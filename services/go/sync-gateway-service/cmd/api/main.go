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

type CreateSyncJobRequest struct {
	Mode       string `json:"mode"`
	Hospital   string `json:"hospital"`
	PayloadRef string `json:"payload_ref"`
}

type SyncJob struct {
	ID         string    `json:"id"`
	Mode       string    `json:"mode"`
	Hospital   string    `json:"hospital"`
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

type ConsumerDeadLetter struct {
	MessageID string    `json:"message_id"`
	Topic     string    `json:"topic"`
	Payload   string    `json:"payload"`
	Error     string    `json:"error"`
	CreatedAt time.Time `json:"created_at"`
}

const (
	jobsRedisKey                = "sync-gateway:jobs"
	outboxPendingRedisKey       = "sync-gateway:outbox:pending"
	outboxDeadLetterRedisKey    = "sync-gateway:outbox:dead_letters"
	outboxProcessedRedisSetKey  = "sync-gateway:outbox:processed"
	consumerProcessedRedisSet   = "sync-gateway:consumer:processed"
	consumerDeadLetterRedisList = "sync-gateway:consumer:dead_letters"
)

var (
	jobs              = map[string]SyncJob{}
	jobsMu            sync.RWMutex
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

func randomID() (string, error) {
	raw := make([]byte, 12)
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

func authz(jwtSecret string) gin.HandlerFunc {
	return func(c *gin.Context) {
		if c.Request.Method == http.MethodGet || jwtSecret == "" {
			c.Next()
			return
		}
		token := bearerToken(c.GetHeader("Authorization"))
		if token == "" {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "missing bearer token"})
			return
		}
		parsed, err := jwt.Parse(token, func(t *jwt.Token) (any, error) {
			if _, ok := t.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, fmt.Errorf("invalid signing method")
			}
			return []byte(jwtSecret), nil
		})
		if err != nil || !parsed.Valid {
			c.AbortWithStatusJSON(http.StatusUnauthorized, gin.H{"error": "invalid token"})
			return
		}
		claims, _ := parsed.Claims.(jwt.MapClaims)
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

func loadJobs(ctx context.Context, rdb *redis.Client) {
	if rdb == nil {
		return
	}
	raw, err := rdb.Get(ctx, jobsRedisKey).Result()
	if err != nil {
		return
	}
	var restored map[string]SyncJob
	if json.Unmarshal([]byte(raw), &restored) != nil {
		return
	}
	jobsMu.Lock()
	jobs = restored
	jobsMu.Unlock()
}

func persistJobs(ctx context.Context, rdb *redis.Client) {
	if rdb == nil {
		return
	}
	jobsMu.RLock()
	copyMap := make(map[string]SyncJob, len(jobs))
	for k, v := range jobs {
		copyMap[k] = v
	}
	jobsMu.RUnlock()
	raw, err := json.Marshal(copyMap)
	if err != nil {
		return
	}
	_ = rdb.Set(ctx, jobsRedisKey, raw, 0).Err()
}

func enqueueOutbox(ctx context.Context, rdb *redis.Client, evt OutboxEvent) {
	if rdb == nil {
		_ = publishKafka(ctx, evt)
		return
	}
	raw, _ := json.Marshal(evt)
	_ = rdb.RPush(ctx, outboxPendingRedisKey, raw).Err()
}

func publishKafka(ctx context.Context, evt OutboxEvent) error {
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

func startOutboxPump(ctx context.Context, rdb *redis.Client) {
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
			if err := publishKafka(ctx, evt); err == nil {
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

func startSyncCommandConsumer(ctx context.Context, rdb *redis.Client) {
	brokers := os.Getenv("KAFKA_BROKERS")
	if brokers == "" {
		return
	}
	groupID := os.Getenv("SYNC_CONSUMER_GROUP")
	if groupID == "" {
		groupID = "sync-gateway-consumer"
	}
	reader := kafka.NewReader(kafka.ReaderConfig{
		Brokers:  strings.Split(brokers, ","),
		GroupID:  groupID,
		Topic:    "sync.commands",
		MinBytes: 1,
		MaxBytes: 10e6,
	})
	go func() {
		defer reader.Close()
		for {
			msg, err := reader.FetchMessage(ctx)
			if err != nil {
				time.Sleep(1 * time.Second)
				continue
			}
			messageID := string(msg.Key)
			if messageID == "" {
				messageID = fmt.Sprintf("offset-%d", msg.Offset)
			}
			if rdb != nil {
				done, _ := rdb.SIsMember(ctx, consumerProcessedRedisSet, messageID).Result()
				if done {
					_ = reader.CommitMessages(ctx, msg)
					continue
				}
			}

			var payload map[string]any
			if err := json.Unmarshal(msg.Value, &payload); err != nil {
				if rdb != nil {
					dlq, _ := json.Marshal(ConsumerDeadLetter{
						MessageID: messageID,
						Topic:     msg.Topic,
						Payload:   string(msg.Value),
						Error:     "invalid json",
						CreatedAt: time.Now().UTC(),
					})
					_ = rdb.RPush(ctx, consumerDeadLetterRedisList, dlq).Err()
				}
				_ = reader.CommitMessages(ctx, msg)
				continue
			}

			cmdType, _ := payload["type"].(string)
			if cmdType == "sync.replay.request" {
				id, _ := randomID()
				now := time.Now().UTC()
				job := SyncJob{
					ID:        id,
					Mode:      "export",
					Hospital:  "replay",
					Status:    "queued",
					TenantID:  fmt.Sprint(payload["tenant_id"]),
					CreatedAt: now,
					UpdatedAt: now,
				}
				jobsMu.Lock()
				jobs[id] = job
				jobsMu.Unlock()
				persistJobs(ctx, rdb)
			}
			if rdb != nil {
				_ = rdb.SAdd(ctx, consumerProcessedRedisSet, messageID).Err()
			}
			_ = reader.CommitMessages(ctx, msg)
		}
	}()
}

func main() {
	const serviceName = "sync-gateway-service"
	prometheus.MustRegister(httpRequestsTotal, httpRequestDuration)
	ctx := context.Background()

	var rdb *redis.Client
	if redisURL := strings.TrimSpace(os.Getenv("REDIS_URL")); redisURL != "" {
		if opt, err := redis.ParseURL(redisURL); err == nil {
			rdb = redis.NewClient(opt)
		}
	}
	loadJobs(ctx, rdb)
	startOutboxPump(ctx, rdb)
	startSyncCommandConsumer(ctx, rdb)

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
	r.Use(authz(strings.TrimSpace(os.Getenv("INTERNAL_JWT_SECRET"))))

	r.POST("/v1/sync/jobs", func(c *gin.Context) {
		var req CreateSyncJobRequest
		if err := c.ShouldBindJSON(&req); err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "invalid request"})
			return
		}
		if req.Mode != "export" && req.Mode != "import" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "mode must be export or import"})
			return
		}
		if req.Hospital == "" {
			c.JSON(http.StatusBadRequest, gin.H{"error": "hospital is required"})
			return
		}

		id, err := randomID()
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "id generation failed"})
			return
		}

		tenantID, _ := c.Get("tenant_id")
		now := time.Now().UTC()
		job := SyncJob{
			ID:         id,
			Mode:       req.Mode,
			Hospital:   req.Hospital,
			PayloadRef: req.PayloadRef,
			Status:     "queued",
			TenantID:   fmt.Sprint(tenantID),
			CreatedAt:  now,
			UpdatedAt:  now,
		}

		jobsMu.Lock()
		jobs[id] = job
		jobsMu.Unlock()
		persistJobs(ctx, rdb)

		payload, _ := json.Marshal(gin.H{
			"id":         id,
			"type":       "sync.created",
			"sync_id":    job.ID,
			"mode":       job.Mode,
			"hospital":   job.Hospital,
			"status":     job.Status,
			"tenant_id":  job.TenantID,
			"created_at": job.CreatedAt,
		})
		enqueueOutbox(ctx, rdb, OutboxEvent{
			ID:        id,
			Topic:     "sync.events",
			Key:       id,
			Payload:   payload,
			CreatedAt: now,
			TenantID:  job.TenantID,
		})

		c.JSON(http.StatusCreated, job)
	})

	r.POST("/v1/sync/jobs/:id/start", func(c *gin.Context) {
		id := c.Param("id")
		jobsMu.Lock()
		job, ok := jobs[id]
		if !ok {
			jobsMu.Unlock()
			c.JSON(http.StatusNotFound, gin.H{"error": "job not found"})
			return
		}
		job.Status = "running"
		job.UpdatedAt = time.Now().UTC()
		jobs[id] = job
		jobsMu.Unlock()
		persistJobs(ctx, rdb)

		payload, _ := json.Marshal(gin.H{
			"id":         fmt.Sprintf("%s-running", job.ID),
			"type":       "sync.running",
			"sync_id":    job.ID,
			"status":     job.Status,
			"tenant_id":  job.TenantID,
			"updated_at": job.UpdatedAt,
		})
		enqueueOutbox(ctx, rdb, OutboxEvent{
			ID:        fmt.Sprintf("%s-running", job.ID),
			Topic:     "sync.events",
			Key:       job.ID,
			Payload:   payload,
			CreatedAt: job.UpdatedAt,
			TenantID:  job.TenantID,
		})
		c.JSON(http.StatusOK, job)
	})

	r.POST("/v1/sync/jobs/:id/complete", func(c *gin.Context) {
		id := c.Param("id")
		jobsMu.Lock()
		job, ok := jobs[id]
		if !ok {
			jobsMu.Unlock()
			c.JSON(http.StatusNotFound, gin.H{"error": "job not found"})
			return
		}
		job.Status = "completed"
		job.UpdatedAt = time.Now().UTC()
		jobs[id] = job
		jobsMu.Unlock()
		persistJobs(ctx, rdb)

		payload, _ := json.Marshal(gin.H{
			"id":         fmt.Sprintf("%s-complete", job.ID),
			"type":       "sync.completed",
			"sync_id":    job.ID,
			"status":     job.Status,
			"tenant_id":  job.TenantID,
			"updated_at": job.UpdatedAt,
		})
		enqueueOutbox(ctx, rdb, OutboxEvent{
			ID:        fmt.Sprintf("%s-complete", job.ID),
			Topic:     "sync.events",
			Key:       job.ID,
			Payload:   payload,
			CreatedAt: job.UpdatedAt,
			TenantID:  job.TenantID,
		})
		c.JSON(http.StatusOK, job)
	})

	r.GET("/v1/sync/jobs/:id", func(c *gin.Context) {
		id := c.Param("id")
		jobsMu.RLock()
		job, ok := jobs[id]
		jobsMu.RUnlock()
		if !ok {
			c.JSON(http.StatusNotFound, gin.H{"error": "job not found"})
			return
		}
		c.JSON(http.StatusOK, job)
	})

	r.GET("/v1/outbox/status", func(c *gin.Context) {
		if rdb == nil {
			c.JSON(http.StatusOK, gin.H{"redis_configured": false})
			return
		}
		pending, _ := rdb.LLen(ctx, outboxPendingRedisKey).Result()
		dead, _ := rdb.LLen(ctx, outboxDeadLetterRedisKey).Result()
		done, _ := rdb.SCard(ctx, outboxProcessedRedisSetKey).Result()
		consumerDone, _ := rdb.SCard(ctx, consumerProcessedRedisSet).Result()
		consumerDead, _ := rdb.LLen(ctx, consumerDeadLetterRedisList).Result()
		c.JSON(http.StatusOK, gin.H{
			"redis_configured": true,
			"pending":          pending,
			"dead_letters":     dead,
			"processed":        done,
			"consumer_done":    consumerDone,
			"consumer_dead":    consumerDead,
		})
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

	r.POST("/v1/consumer/replay-deadletters", func(c *gin.Context) {
		if rdb == nil {
			c.JSON(http.StatusOK, gin.H{"replayed": 0, "redis_configured": false})
			return
		}
		replayed := 0
		for {
			raw, err := rdb.LPop(ctx, consumerDeadLetterRedisList).Result()
			if err != nil || raw == "" {
				break
			}
			var dlq ConsumerDeadLetter
			if json.Unmarshal([]byte(raw), &dlq) != nil {
				continue
			}
			_ = rdb.SRem(ctx, consumerProcessedRedisSet, dlq.MessageID).Err()
			replayed++
		}
		c.JSON(http.StatusOK, gin.H{"replayed": replayed})
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "sync-gateway-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8085")
}
