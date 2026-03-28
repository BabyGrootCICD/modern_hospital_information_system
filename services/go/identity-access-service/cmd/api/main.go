package main

import (
	"context"
	"crypto/rand"
	"encoding/base64"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
	"strings"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
	"github.com/redis/go-redis/v9"
	"github.com/segmentio/kafka-go"
)

type LoginRequest struct {
	Username string `json:"username"`
	Role     string `json:"role"`
	Tenant   string `json:"tenant"`
}

type Session struct {
	Token     string    `json:"token"`
	Username  string    `json:"username"`
	Role      string    `json:"role"`
	Tenant    string    `json:"tenant"`
	ExpiresAt time.Time `json:"expires_at"`
}

type OutboxEvent struct {
	ID          string          `json:"id"`
	Topic       string          `json:"topic"`
	Key         string          `json:"key"`
	Payload     json.RawMessage `json:"payload"`
	Attempts    int             `json:"attempts"`
	CreatedAt   time.Time       `json:"created_at"`
	LastError   string          `json:"last_error,omitempty"`
	Correlation string          `json:"correlation,omitempty"`
	TenantID    string          `json:"tenant_id,omitempty"`
}

type AppEnv struct {
	redis            *redis.Client
	outboxMaxAttempt int
	jwtSecrets       []string
	supabaseURL      string
	supabaseKey      string
}

const (
	sessionsRedisKey         = "identity:sessions"
	outboxPendingRedisKey    = "identity:outbox:pending"
	outboxDeadLetterRedisKey = "identity:outbox:dead_letters"
	outboxProcessedRedisKey  = "identity:outbox:processed"
)

var (
	sessions          = map[string]Session{}
	sessionsMu        sync.RWMutex
	httpRequestsTotal = prometheus.NewCounterVec(
		prometheus.CounterOpts{
			Name: "http_requests_total",
			Help: "Total HTTP requests.",
		},
		[]string{"service", "method", "route", "status"},
	)
	httpRequestDuration = prometheus.NewHistogramVec(
		prometheus.HistogramOpts{
			Name:    "http_request_duration_seconds",
			Help:    "HTTP request duration in seconds.",
			Buckets: prometheus.DefBuckets,
		},
		[]string{"service", "method", "route"},
	)
)

func generateToken() (string, error) {
	raw := make([]byte, 32)
	if _, err := rand.Read(raw); err != nil {
		return "", err
	}
	return base64.RawURLEncoding.EncodeToString(raw), nil
}

func bearerToken(header string) string {
	parts := strings.SplitN(header, " ", 2)
	if len(parts) != 2 || !strings.EqualFold(parts[0], "Bearer") {
		return ""
	}
	return strings.TrimSpace(parts[1])
}

func jwtSecretsFromEnv() []string {
	seen := map[string]struct{}{}
	var out []string
	add := func(v string) {
		v = strings.TrimSpace(v)
		if v == "" {
			return
		}
		if _, ok := seen[v]; ok {
			return
		}
		seen[v] = struct{}{}
		out = append(out, v)
	}
	add(os.Getenv("INTERNAL_JWT_SECRET"))
	add(os.Getenv("INTERNAL_JWT_SECRET_PREVIOUS"))
	for _, item := range strings.Split(os.Getenv("INTERNAL_JWT_SECRETS"), ",") {
		add(item)
	}
	return out
}

func parseJWTWithAnySecret(token string, secrets []string) (jwt.MapClaims, bool) {
	for _, secret := range secrets {
		parsed, err := jwt.Parse(token, func(t *jwt.Token) (any, error) {
			if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, fmt.Errorf("unexpected signing method")
			}
			return []byte(secret), nil
		})
		if err != nil || !parsed.Valid {
			continue
		}
		claims, ok := parsed.Claims.(jwt.MapClaims)
		if ok {
			return claims, true
		}
	}
	return nil, false
}

func persistSessionToSupabase(ctx context.Context, env *AppEnv, s Session) {
	if env.supabaseURL == "" || env.supabaseKey == "" {
		return
	}
	endpoint := fmt.Sprintf("%s/rest/v1/identity_sessions", strings.TrimRight(env.supabaseURL, "/"))
	payload := []map[string]any{{
		"token":      s.Token,
		"username":   s.Username,
		"role":       s.Role,
		"tenant_id":  s.Tenant,
		"expires_at": s.ExpiresAt.UTC().Format(time.RFC3339),
	}}
	body, _ := json.Marshal(payload)
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, endpoint, strings.NewReader(string(body)))
	if err != nil {
		return
	}
	req.Header.Set("apikey", env.supabaseKey)
	req.Header.Set("Authorization", "Bearer "+env.supabaseKey)
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Prefer", "resolution=merge-duplicates,return=minimal")
	_, _ = http.DefaultClient.Do(req)
}

func persistOutboxEventToSupabase(ctx context.Context, env *AppEnv, evt OutboxEvent) {
	if env.supabaseURL == "" || env.supabaseKey == "" {
		return
	}
	endpoint := fmt.Sprintf("%s/rest/v1/service_outbox_events", strings.TrimRight(env.supabaseURL, "/"))
	payload := []map[string]any{{
		"event_id":    evt.ID,
		"service":     "identity-access-service",
		"topic":       evt.Topic,
		"event_key":   evt.Key,
		"payload":     string(evt.Payload),
		"tenant_id":   evt.TenantID,
		"attempts":    evt.Attempts,
		"created_at":  evt.CreatedAt.UTC().Format(time.RFC3339),
		"status":      "pending",
		"last_error":  evt.LastError,
		"correlation": evt.Correlation,
	}}
	body, _ := json.Marshal(payload)
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, endpoint, strings.NewReader(string(body)))
	if err != nil {
		return
	}
	req.Header.Set("apikey", env.supabaseKey)
	req.Header.Set("Authorization", "Bearer "+env.supabaseKey)
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Prefer", "resolution=merge-duplicates,return=minimal")
	_, _ = http.DefaultClient.Do(req)
}

func loadSessions(ctx context.Context, env *AppEnv) {
	if env.redis == nil {
		return
	}
	raw, err := env.redis.Get(ctx, sessionsRedisKey).Result()
	if err != nil {
		return
	}
	var restored map[string]Session
	if err := json.Unmarshal([]byte(raw), &restored); err != nil {
		return
	}
	sessionsMu.Lock()
	sessions = restored
	sessionsMu.Unlock()
}

func persistSessions(ctx context.Context, env *AppEnv) {
	if env.redis == nil {
		return
	}
	sessionsMu.RLock()
	copyMap := make(map[string]Session, len(sessions))
	for k, v := range sessions {
		copyMap[k] = v
	}
	sessionsMu.RUnlock()
	raw, err := json.Marshal(copyMap)
	if err != nil {
		return
	}
	_ = env.redis.Set(ctx, sessionsRedisKey, raw, 0).Err()
}

func enqueueOutbox(ctx context.Context, env *AppEnv, topic, key, tenantID string, payload any) {
	encoded, err := json.Marshal(payload)
	if err != nil {
		log.Printf("failed to marshal event payload: %v", err)
		return
	}
	id, err := generateToken()
	if err != nil {
		log.Printf("failed to generate event id: %v", err)
		return
	}
	evt := OutboxEvent{
		ID:        id,
		Topic:     topic,
		Key:       key,
		Payload:   encoded,
		Attempts:  0,
		CreatedAt: time.Now().UTC(),
		TenantID:  tenantID,
	}
	raw, err := json.Marshal(evt)
	if err != nil {
		return
	}
	if env.redis == nil {
		publishOutboxEvent(ctx, env, evt)
		return
	}
	persistOutboxEventToSupabase(ctx, env, evt)
	if err := env.redis.RPush(ctx, outboxPendingRedisKey, raw).Err(); err != nil {
		log.Printf("failed to enqueue outbox event: %v", err)
	}
}

func publishOutboxEvent(ctx context.Context, env *AppEnv, evt OutboxEvent) error {
	brokers := os.Getenv("KAFKA_BROKERS")
	if brokers == "" {
		return nil
	}
	writer := &kafka.Writer{
		Addr:     kafka.TCP(strings.Split(brokers, ",")...),
		Topic:    evt.Topic,
		Balancer: &kafka.LeastBytes{},
	}
	defer writer.Close()

	msgKey := evt.Key
	if msgKey == "" {
		msgKey = evt.ID
	}
	timeoutCtx, cancel := context.WithTimeout(ctx, 3*time.Second)
	defer cancel()
	return writer.WriteMessages(timeoutCtx, kafka.Message{
		Key: []byte(msgKey), Value: evt.Payload,
		Headers: []kafka.Header{
			{Key: "event_id", Value: []byte(evt.ID)},
			{Key: "tenant_id", Value: []byte(evt.TenantID)},
		},
	})
}

func startOutboxPump(ctx context.Context, env *AppEnv) {
	if env.redis == nil {
		return
	}
	go func() {
		ticker := time.NewTicker(2 * time.Second)
		defer ticker.Stop()
		for {
			select {
			case <-ctx.Done():
				return
			case <-ticker.C:
				processOutboxOnce(ctx, env)
			}
		}
	}()
}

func processOutboxOnce(ctx context.Context, env *AppEnv) {
	if env.redis == nil {
		return
	}
	raw, err := env.redis.LPop(ctx, outboxPendingRedisKey).Result()
	if err != nil || raw == "" {
		return
	}
	var evt OutboxEvent
	if err := json.Unmarshal([]byte(raw), &evt); err != nil {
		return
	}
	alreadyDone, _ := env.redis.SIsMember(ctx, outboxProcessedRedisKey, evt.ID).Result()
	if alreadyDone {
		return
	}
	if err := publishOutboxEvent(ctx, env, evt); err == nil {
		_ = env.redis.SAdd(ctx, outboxProcessedRedisKey, evt.ID).Err()
		return
	}
	evt.Attempts++
	if evt.Attempts >= env.outboxMaxAttempt {
		evt.LastError = "max retry attempts reached"
		data, _ := json.Marshal(evt)
		_ = env.redis.RPush(ctx, outboxDeadLetterRedisKey, data).Err()
		return
	}
	data, _ := json.Marshal(evt)
	_ = env.redis.RPush(ctx, outboxPendingRedisKey, data).Err()
}

func authzMiddleware(env *AppEnv) gin.HandlerFunc {
	return func(c *gin.Context) {
		if c.Request.Method == http.MethodGet {
			c.Next()
			return
		}
		if c.FullPath() == "/v1/auth/login" {
			c.Next()
			return
		}
		if len(env.jwtSecrets) == 0 {
			c.Next()
			return
		}
		token := bearerToken(c.GetHeader("Authorization"))
		if token == "" {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "missing bearer token"})
			return
		}
		claims, ok := parseJWTWithAnySecret(token, env.jwtSecrets)
		if !ok {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "invalid token"})
			return
		}
		tenantClaim, _ := claims["tenant"].(string)
		tenantHeader := strings.TrimSpace(c.GetHeader("X-Tenant-ID"))
		tenantID := tenantClaim
		if tenantID == "" {
			tenantID = tenantHeader
		}
		if tenantID == "" || (tenantHeader != "" && tenantClaim != "" && tenantHeader != tenantClaim) {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "tenant mismatch or missing tenant"})
			return
		}
		c.Set("tenant_id", tenantID)
		c.Next()
	}
}

func main() {
	const serviceName = "identity-access-service"
	prometheus.MustRegister(httpRequestsTotal, httpRequestDuration)

	ctx := context.Background()
	var redisClient *redis.Client
	if redisAddr := strings.TrimSpace(os.Getenv("REDIS_URL")); redisAddr != "" {
		opt, err := redis.ParseURL(redisAddr)
		if err == nil {
			redisClient = redis.NewClient(opt)
		}
	}
	env := &AppEnv{
		redis:            redisClient,
		outboxMaxAttempt: 5,
		jwtSecrets:       jwtSecretsFromEnv(),
		supabaseURL:      strings.TrimSpace(os.Getenv("SUPABASE_URL")),
		supabaseKey:      strings.TrimSpace(os.Getenv("SUPABASE_SERVICE_KEY")),
	}

	loadSessions(ctx, env)
	startOutboxPump(ctx, env)

	r := gin.New()
	r.Use(gin.Recovery())
	r.Use(func(c *gin.Context) {
		start := time.Now()
		c.Next()

		route := c.FullPath()
		if route == "" {
			route = "unmatched"
		}
		status := fmt.Sprintf("%d", c.Writer.Status())
		httpRequestsTotal.WithLabelValues(serviceName, c.Request.Method, route, status).Inc()
		httpRequestDuration.WithLabelValues(serviceName, c.Request.Method, route).Observe(time.Since(start).Seconds())
	})
	r.Use(authzMiddleware(env))

	r.POST("/v1/auth/login", func(c *gin.Context) {
		var req LoginRequest
		if err := c.ShouldBindJSON(&req); err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "invalid request"})
			return
		}
		if req.Username == "" || req.Role == "" || req.Tenant == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "username, role, and tenant are required"})
			return
		}

		token, err := generateToken()
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "token generation failed"})
			return
		}

		session := Session{
			Token:     token,
			Username:  req.Username,
			Role:      req.Role,
			Tenant:    req.Tenant,
			ExpiresAt: time.Now().UTC().Add(8 * time.Hour),
		}

		sessionsMu.Lock()
		sessions[token] = session
		sessionsMu.Unlock()
		persistSessions(ctx, env)
		persistSessionToSupabase(ctx, env, session)

		enqueueOutbox(ctx, env, "identity.events", session.Token, session.Tenant, gin.H{
			"id":         token,
			"type":       "auth.login",
			"username":   session.Username,
			"role":       session.Role,
			"tenant":     session.Tenant,
			"created_at": time.Now().UTC(),
		})

		c.JSON(http.StatusOK, session)
	})

	r.GET("/v1/auth/validate", func(c *gin.Context) {
		token := bearerToken(c.GetHeader("Authorization"))
		if token == "" {
			c.JSON(http.StatusUnauthorized, gin.H{"valid": false, "error": "missing bearer token"})
			return
		}

		sessionsMu.RLock()
		session, ok := sessions[token]
		sessionsMu.RUnlock()
		if !ok || time.Now().UTC().After(session.ExpiresAt) {
			c.JSON(http.StatusUnauthorized, gin.H{"valid": false})
			return
		}

		c.JSON(http.StatusOK, gin.H{
			"valid":   true,
			"subject": session.Username,
			"role":    session.Role,
			"tenant":  session.Tenant,
		})
	})

	r.GET("/v1/rbac/policies/:role", func(c *gin.Context) {
		role := c.Param("role")
		policies := []string{"dashboard.read"}
		switch role {
		case "admin":
			policies = append(policies, "admin.read", "admin.write", "report.read")
		case "doctor":
			policies = append(policies, "order.read", "order.write", "chart.read")
		case "auditor":
			policies = append(policies, "audit.read", "report.read")
		}

		c.JSON(http.StatusOK, gin.H{"role": role, "policies": policies})
	})

	r.GET("/v1/outbox/status", func(c *gin.Context) {
		if env.redis == nil {
			c.JSON(http.StatusOK, gin.H{"redis_configured": false})
			return
		}
		pending, _ := env.redis.LLen(ctx, outboxPendingRedisKey).Result()
		dead, _ := env.redis.LLen(ctx, outboxDeadLetterRedisKey).Result()
		done, _ := env.redis.SCard(ctx, outboxProcessedRedisKey).Result()
		c.JSON(http.StatusOK, gin.H{
			"redis_configured": true,
			"pending":          pending,
			"dead_letters":     dead,
			"processed":        done,
		})
	})

	r.POST("/v1/outbox/replay-deadletters", func(c *gin.Context) {
		if env.redis == nil {
			c.JSON(http.StatusOK, gin.H{"moved": 0, "redis_configured": false})
			return
		}
		moved := 0
		for {
			raw, err := env.redis.LPop(ctx, outboxDeadLetterRedisKey).Result()
			if err != nil || raw == "" {
				break
			}
			var evt OutboxEvent
			if json.Unmarshal([]byte(raw), &evt) != nil {
				continue
			}
			evt.Attempts = 0
			evt.LastError = ""
			data, _ := json.Marshal(evt)
			_ = env.redis.RPush(ctx, outboxPendingRedisKey, data).Err()
			moved++
		}
		c.JSON(http.StatusOK, gin.H{"moved": moved})
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "identity-access-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8081")
}
