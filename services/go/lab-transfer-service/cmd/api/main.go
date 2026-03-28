package main

import (
	"context"
	"crypto/rand"
	"encoding/hex"
	"encoding/json"
	"fmt"
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

type CreateTransferRequest struct {
	PatientID  string `json:"patient_id"`
	SourceSite string `json:"source_site"`
	TargetSite string `json:"target_site"`
	PayloadRef string `json:"payload_ref"`
}

type Transfer struct {
	ID         string    `json:"id"`
	PatientID  string    `json:"patient_id"`
	SourceSite string    `json:"source_site"`
	TargetSite string    `json:"target_site"`
	PayloadRef string    `json:"payload_ref"`
	Status     string    `json:"status"`
	TenantID   string    `json:"tenant_id"`
	CreatedAt  time.Time `json:"created_at"`
	UpdatedAt  time.Time `json:"updated_at"`
}

type OutboxEvent struct {
	ID        string          `json:"id"`
	Topic     string          `json:"topic"`
	Key       string          `json:"key"`
	Payload   json.RawMessage `json:"payload"`
	Attempts  int             `json:"attempts"`
	CreatedAt time.Time       `json:"created_at"`
	TenantID  string          `json:"tenant_id,omitempty"`
}

var (
	transfers         = map[string]Transfer{}
	transfersMu       sync.RWMutex
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

const (
	transfersRedisKey          = "lab-transfer:transfers"
	outboxPendingRedisKey      = "lab-transfer:outbox:pending"
	outboxDeadLetterRedisKey   = "lab-transfer:outbox:dead_letters"
	outboxProcessedRedisSetKey = "lab-transfer:outbox:processed"
)

func randomHex(size int) (string, error) {
	raw := make([]byte, size)
	if _, err := rand.Read(raw); err != nil {
		return "", err
	}
	return hex.EncodeToString(raw), nil
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
				return nil, fmt.Errorf("invalid signing method")
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

func authz(jwtSecrets []string) gin.HandlerFunc {
	return func(c *gin.Context) {
		if c.Request.Method == http.MethodGet || len(jwtSecrets) == 0 {
			c.Next()
			return
		}
		token := bearerToken(c.GetHeader("Authorization"))
		if token == "" {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "missing bearer token"})
			return
		}
		claims, ok := parseJWTWithAnySecret(token, jwtSecrets)
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
		if tenantID == "" || (tenantClaim != "" && tenantHeader != "" && tenantClaim != tenantHeader) {
			c.AbortWithStatusJSON(http.StatusForbidden, gin.H{"error": "tenant mismatch"})
			return
		}
		c.Set("tenant_id", tenantID)
		c.Next()
	}
}

func loadTransfers(ctx context.Context, rdb *redis.Client) {
	if rdb == nil {
		return
	}
	raw, err := rdb.Get(ctx, transfersRedisKey).Result()
	if err != nil {
		return
	}
	var restored map[string]Transfer
	if json.Unmarshal([]byte(raw), &restored) != nil {
		return
	}
	transfersMu.Lock()
	transfers = restored
	transfersMu.Unlock()
}

func persistTransfers(ctx context.Context, rdb *redis.Client) {
	if rdb == nil {
		return
	}
	transfersMu.RLock()
	copyMap := make(map[string]Transfer, len(transfers))
	for k, v := range transfers {
		copyMap[k] = v
	}
	transfersMu.RUnlock()
	raw, err := json.Marshal(copyMap)
	if err != nil {
		return
	}
	_ = rdb.Set(ctx, transfersRedisKey, raw, 0).Err()
}

func enqueueOutbox(ctx context.Context, rdb *redis.Client, evt OutboxEvent) {
	if rdb == nil {
		_ = publishToKafka(ctx, evt)
		return
	}
	raw, _ := json.Marshal(evt)
	_ = rdb.RPush(ctx, outboxPendingRedisKey, raw).Err()
	persistOutboxEventToSupabase(ctx, evt)
}

func persistTransfersToSupabase(ctx context.Context, transfer Transfer) {
	base := strings.TrimSpace(os.Getenv("SUPABASE_URL"))
	key := strings.TrimSpace(os.Getenv("SUPABASE_SERVICE_KEY"))
	if base == "" || key == "" {
		return
	}
	endpoint := fmt.Sprintf("%s/rest/v1/lab_transfers", strings.TrimRight(base, "/"))
	payload := []map[string]any{{
		"id":          transfer.ID,
		"patient_id":  transfer.PatientID,
		"source_site": transfer.SourceSite,
		"target_site": transfer.TargetSite,
		"payload_ref": transfer.PayloadRef,
		"status":      transfer.Status,
		"tenant_id":   transfer.TenantID,
		"created_at":  transfer.CreatedAt.UTC().Format(time.RFC3339),
		"updated_at":  transfer.UpdatedAt.UTC().Format(time.RFC3339),
	}}
	body, _ := json.Marshal(payload)
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, endpoint, strings.NewReader(string(body)))
	if err != nil {
		return
	}
	req.Header.Set("apikey", key)
	req.Header.Set("Authorization", "Bearer "+key)
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Prefer", "resolution=merge-duplicates,return=minimal")
	_, _ = http.DefaultClient.Do(req)
}

func persistOutboxEventToSupabase(ctx context.Context, evt OutboxEvent) {
	base := strings.TrimSpace(os.Getenv("SUPABASE_URL"))
	key := strings.TrimSpace(os.Getenv("SUPABASE_SERVICE_KEY"))
	if base == "" || key == "" {
		return
	}
	endpoint := fmt.Sprintf("%s/rest/v1/service_outbox_events", strings.TrimRight(base, "/"))
	payload := []map[string]any{{
		"event_id":   evt.ID,
		"service":    "lab-transfer-service",
		"topic":      evt.Topic,
		"event_key":  evt.Key,
		"payload":    string(evt.Payload),
		"tenant_id":  evt.TenantID,
		"attempts":   evt.Attempts,
		"created_at": evt.CreatedAt.UTC().Format(time.RFC3339),
		"status":     "pending",
	}}
	body, _ := json.Marshal(payload)
	req, err := http.NewRequestWithContext(ctx, http.MethodPost, endpoint, strings.NewReader(string(body)))
	if err != nil {
		return
	}
	req.Header.Set("apikey", key)
	req.Header.Set("Authorization", "Bearer "+key)
	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("Prefer", "resolution=merge-duplicates,return=minimal")
	_, _ = http.DefaultClient.Do(req)
}

func publishToKafka(ctx context.Context, evt OutboxEvent) error {
	brokers := os.Getenv("KAFKA_BROKERS")
	if brokers == "" {
		return nil
	}
	writer := kafka.Writer{
		Addr:     kafka.TCP(strings.Split(brokers, ",")...),
		Topic:    evt.Topic,
		Balancer: &kafka.LeastBytes{},
	}
	defer writer.Close()
	timeoutCtx, cancel := context.WithTimeout(ctx, 3*time.Second)
	defer cancel()
	return writer.WriteMessages(timeoutCtx, kafka.Message{
		Key:   []byte(evt.Key),
		Value: evt.Payload,
		Headers: []kafka.Header{
			{Key: "event_id", Value: []byte(evt.ID)},
			{Key: "tenant_id", Value: []byte(evt.TenantID)},
		},
	})
}

func runOutboxPump(ctx context.Context, rdb *redis.Client) {
	if rdb == nil {
		return
	}
	go func() {
		ticker := time.NewTicker(2 * time.Second)
		defer ticker.Stop()
		for range ticker.C {
			raw, err := rdb.LPop(ctx, outboxPendingRedisKey).Result()
			if err != nil || raw == "" {
				continue
			}
			var evt OutboxEvent
			if json.Unmarshal([]byte(raw), &evt) != nil {
				continue
			}
			done, _ := rdb.SIsMember(ctx, outboxProcessedRedisSetKey, evt.ID).Result()
			if done {
				continue
			}
			if err := publishToKafka(ctx, evt); err == nil {
				_ = rdb.SAdd(ctx, outboxProcessedRedisSetKey, evt.ID).Err()
				continue
			}
			evt.Attempts++
			data, _ := json.Marshal(evt)
			if evt.Attempts >= 5 {
				_ = rdb.RPush(ctx, outboxDeadLetterRedisKey, data).Err()
			} else {
				_ = rdb.RPush(ctx, outboxPendingRedisKey, data).Err()
			}
		}
	}()
}

func main() {
	const serviceName = "lab-transfer-service"
	prometheus.MustRegister(httpRequestsTotal, httpRequestDuration)
	ctx := context.Background()

	var rdb *redis.Client
	if redisURL := strings.TrimSpace(os.Getenv("REDIS_URL")); redisURL != "" {
		if opt, err := redis.ParseURL(redisURL); err == nil {
			rdb = redis.NewClient(opt)
		}
	}
	loadTransfers(ctx, rdb)
	runOutboxPump(ctx, rdb)

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
	r.Use(authz(jwtSecretsFromEnv()))

	r.POST("/v1/transfers", func(c *gin.Context) {
		var req CreateTransferRequest
		if err := c.ShouldBindJSON(&req); err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "invalid request"})
			return
		}
		if req.PatientID == "" || req.SourceSite == "" || req.TargetSite == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "patient_id, source_site, target_site are required"})
			return
		}
		tenantID, _ := c.Get("tenant_id")
		id, err := randomHex(12)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "id generation failed"})
			return
		}
		now := time.Now().UTC()
		transfer := Transfer{
			ID:         id,
			PatientID:  req.PatientID,
			SourceSite: req.SourceSite,
			TargetSite: req.TargetSite,
			PayloadRef: req.PayloadRef,
			Status:     "queued",
			TenantID:   fmt.Sprint(tenantID),
			CreatedAt:  now,
			UpdatedAt:  now,
		}
		transfersMu.Lock()
		transfers[id] = transfer
		transfersMu.Unlock()
		persistTransfers(ctx, rdb)
		persistTransfersToSupabase(ctx, transfer)

		payload, _ := json.Marshal(gin.H{
			"id":          id,
			"type":        "transfer.created",
			"transfer_id": transfer.ID,
			"patient_id":  transfer.PatientID,
			"source_site": transfer.SourceSite,
			"target_site": transfer.TargetSite,
			"status":      transfer.Status,
			"tenant_id":   transfer.TenantID,
			"created_at":  transfer.CreatedAt,
		})
		enqueueOutbox(ctx, rdb, OutboxEvent{
			ID:        id,
			Topic:     "transfer.events",
			Key:       id,
			Payload:   payload,
			CreatedAt: now,
			TenantID:  transfer.TenantID,
		})
		c.JSON(http.StatusCreated, transfer)
	})

	r.GET("/v1/transfers/:id", func(c *gin.Context) {
		id := c.Param("id")
		transfersMu.RLock()
		transfer, ok := transfers[id]
		transfersMu.RUnlock()
		if !ok {
			c.JSON(http.StatusNotFound, gin.H{"error": "transfer not found"})
			return
		}
		c.JSON(http.StatusOK, transfer)
	})

	r.POST("/v1/transfers/:id/complete", func(c *gin.Context) {
		id := c.Param("id")
		transfersMu.Lock()
		transfer, ok := transfers[id]
		if !ok {
			transfersMu.Unlock()
			c.JSON(http.StatusNotFound, gin.H{"error": "transfer not found"})
			return
		}
		transfer.Status = "completed"
		transfer.UpdatedAt = time.Now().UTC()
		transfers[id] = transfer
		transfersMu.Unlock()
		persistTransfers(ctx, rdb)
		persistTransfersToSupabase(ctx, transfer)

		payload, _ := json.Marshal(gin.H{
			"id":          fmt.Sprintf("%s-complete", transfer.ID),
			"type":        "transfer.completed",
			"transfer_id": transfer.ID,
			"status":      transfer.Status,
			"tenant_id":   transfer.TenantID,
			"updated_at":  transfer.UpdatedAt,
		})
		enqueueOutbox(ctx, rdb, OutboxEvent{
			ID:        fmt.Sprintf("%s-complete", transfer.ID),
			Topic:     "transfer.events",
			Key:       transfer.ID,
			Payload:   payload,
			CreatedAt: transfer.UpdatedAt,
			TenantID:  transfer.TenantID,
		})

		c.JSON(http.StatusOK, transfer)
	})

	r.GET("/v1/outbox/status", func(c *gin.Context) {
		if rdb == nil {
			c.JSON(http.StatusOK, gin.H{"redis_configured": false})
			return
		}
		pending, _ := rdb.LLen(ctx, outboxPendingRedisKey).Result()
		dead, _ := rdb.LLen(ctx, outboxDeadLetterRedisKey).Result()
		done, _ := rdb.SCard(ctx, outboxProcessedRedisSetKey).Result()
		c.JSON(http.StatusOK, gin.H{"redis_configured": true, "pending": pending, "dead_letters": dead, "processed": done})
	})

	r.POST("/v1/outbox/replay-deadletters", func(c *gin.Context) {
		if rdb == nil {
			c.JSON(http.StatusOK, gin.H{"moved": 0, "redis_configured": false})
			return
		}
		moved := 0
		for {
			raw, err := rdb.LPop(ctx, outboxDeadLetterRedisKey).Result()
			if err != nil || raw == "" {
				break
			}
			var evt OutboxEvent
			if json.Unmarshal([]byte(raw), &evt) != nil {
				continue
			}
			evt.Attempts = 0
			data, _ := json.Marshal(evt)
			_ = rdb.RPush(ctx, outboxPendingRedisKey, data).Err()
			moved++
		}
		c.JSON(http.StatusOK, gin.H{"moved": moved})
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "lab-transfer-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8084")
}
