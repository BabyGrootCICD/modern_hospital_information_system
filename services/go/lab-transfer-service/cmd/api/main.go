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
	CreatedAt  time.Time `json:"created_at"`
	UpdatedAt  time.Time `json:"updated_at"`
}

var (
	transfers   = map[string]Transfer{}
	transfersMu sync.RWMutex
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

func newID() (string, error) {
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
	const serviceName = "lab-transfer-service"
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

		id, err := newID()
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
			CreatedAt:  now,
			UpdatedAt:  now,
		}

		transfersMu.Lock()
		transfers[id] = transfer
		transfersMu.Unlock()
		publishEvent("transfer.events", gin.H{
			"type":        "transfer.created",
			"transfer_id": transfer.ID,
			"patient_id":  transfer.PatientID,
			"source_site": transfer.SourceSite,
			"target_site": transfer.TargetSite,
			"status":      transfer.Status,
			"created_at":  transfer.CreatedAt,
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
		publishEvent("transfer.events", gin.H{
			"type":        "transfer.completed",
			"transfer_id": transfer.ID,
			"status":      transfer.Status,
			"updated_at":  transfer.UpdatedAt,
		})
		c.JSON(http.StatusOK, transfer)
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "lab-transfer-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8084")
}
