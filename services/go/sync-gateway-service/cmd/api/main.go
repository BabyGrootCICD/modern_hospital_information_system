package main

import (
	"context"
	"crypto/rand"
	"encoding/json"
	"encoding/hex"
	"fmt"
	"log"
	"net/http"
	"os"
	"strings"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
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
	CreatedAt  time.Time `json:"created_at"`
	UpdatedAt  time.Time `json:"updated_at"`
}

var (
	jobs   = map[string]SyncJob{}
	jobsMu sync.RWMutex
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

func publishEvent(topic string, payload any) {
	brokers := os.Getenv("KAFKA_BROKERS")
	if brokers == "" {
		return
	}
	encoded, err := json.Marshal(payload)
	if err != nil {
		log.Printf("failed to marshal event: %v", err)
		return
	}

	writer := &kafka.Writer{
		Addr:     kafka.TCP(strings.Split(brokers, ",")...),
		Topic:    topic,
		Balancer: &kafka.LeastBytes{},
	}
	defer writer.Close()

	ctx, cancel := context.WithTimeout(context.Background(), 3*time.Second)
	defer cancel()
	if err := writer.WriteMessages(ctx, kafka.Message{Key: []byte(time.Now().UTC().Format(time.RFC3339Nano)), Value: encoded}); err != nil {
		log.Printf("failed to publish event to %s: %v", topic, err)
	}
}

func main() {
	const serviceName = "sync-gateway-service"
	prometheus.MustRegister(httpRequestsTotal, httpRequestDuration)

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

		now := time.Now().UTC()
		job := SyncJob{
			ID:         id,
			Mode:       req.Mode,
			Hospital:   req.Hospital,
			PayloadRef: req.PayloadRef,
			Status:     "queued",
			CreatedAt:  now,
			UpdatedAt:  now,
		}

		jobsMu.Lock()
		jobs[id] = job
		jobsMu.Unlock()
		publishEvent("sync.events", gin.H{
			"type":       "sync.created",
			"sync_id":    job.ID,
			"mode":       job.Mode,
			"hospital":   job.Hospital,
			"status":     job.Status,
			"created_at": job.CreatedAt,
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
		publishEvent("sync.events", gin.H{
			"type":      "sync.running",
			"sync_id":   job.ID,
			"status":    job.Status,
			"updated_at": job.UpdatedAt,
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
		publishEvent("sync.events", gin.H{
			"type":      "sync.completed",
			"sync_id":   job.ID,
			"status":    job.Status,
			"updated_at": job.UpdatedAt,
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

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "sync-gateway-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8085")
}
