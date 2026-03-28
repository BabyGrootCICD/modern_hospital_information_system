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
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
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

var (
	sessions   = map[string]Session{}
	sessionsMu sync.RWMutex
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
	const serviceName = "identity-access-service"
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
		publishEvent("identity.events", gin.H{
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

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "identity-access-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8081")
}
