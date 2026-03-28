package main

import (
	"fmt"
	"net/http"
	"os"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

var (
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

func bearerToken(header string) string {
	parts := strings.SplitN(header, " ", 2)
	if len(parts) != 2 || !strings.EqualFold(parts[0], "Bearer") {
		return ""
	}
	return strings.TrimSpace(parts[1])
}

func authz(jwtSecret string) gin.HandlerFunc {
	return func(c *gin.Context) {
		if c.FullPath() == "/healthz" || c.FullPath() == "/metrics" {
			c.Next()
			return
		}
		if jwtSecret == "" {
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

func main() {
	const serviceName = "order-clinical-service"
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
	r.Use(authz(strings.TrimSpace(os.Getenv("INTERNAL_JWT_SECRET"))))

	r.GET("/v1/orders/ping", func(c *gin.Context) {
		tenantID, _ := c.Get("tenant_id")
		c.JSON(http.StatusOK, gin.H{"service": "order-clinical-service", "tenant_id": tenantID, "status": "ok"})
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "order-clinical-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8083")
}
