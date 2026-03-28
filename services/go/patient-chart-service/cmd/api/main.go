package main

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"
	"strings"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/golang-jwt/jwt/v5"
	"github.com/prometheus/client_golang/prometheus"
	"github.com/prometheus/client_golang/prometheus/promhttp"
)

type PatientChart struct {
	PatientID  string `json:"patient_id"`
	VisitDate  string `json:"visit_date"`
	Department string `json:"department"`
	DoctorID   string `json:"doctor_id"`
	Summary    string `json:"summary"`
}

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

func fetchFromSupabase(patientID string) ([]PatientChart, error) {
	baseURL := os.Getenv("SUPABASE_URL")
	apiKey := os.Getenv("SUPABASE_ANON_KEY")
	if baseURL == "" || apiKey == "" {
		return nil, fmt.Errorf("supabase env is not set")
	}

	query := url.QueryEscape(patientID)
	endpoint := fmt.Sprintf("%s/rest/v1/patient_charts?patient_id=eq.%s&select=*", baseURL, query)
	req, err := http.NewRequest(http.MethodGet, endpoint, nil)
	if err != nil {
		return nil, err
	}
	req.Header.Set("apikey", apiKey)
	req.Header.Set("Authorization", "Bearer "+apiKey)
	req.Header.Set("Accept", "application/json")

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()
	if resp.StatusCode >= 300 {
		body, _ := io.ReadAll(resp.Body)
		return nil, fmt.Errorf("supabase error: %s", string(body))
	}

	var charts []PatientChart
	if err := json.NewDecoder(resp.Body).Decode(&charts); err != nil {
		return nil, err
	}
	return charts, nil
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
	const serviceName = "patient-chart-service"
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

	r.GET("/v1/charts/:patientID", func(c *gin.Context) {
		patientID := c.Param("patientID")
		charts, err := fetchFromSupabase(patientID)
		if err == nil {
			tenantID, _ := c.Get("tenant_id")
			c.JSON(http.StatusOK, gin.H{"source": "supabase", "tenant_id": tenantID, "items": charts})
			return
		}

		tenantID, _ := c.Get("tenant_id")
		c.JSON(http.StatusOK, gin.H{
			"source":    "fallback-mock",
			"tenant_id": tenantID,
			"items": []PatientChart{
				{
					PatientID:  patientID,
					VisitDate:  "2026-03-28",
					Department: "OPD",
					DoctorID:   "D001",
					Summary:    "No Supabase connection configured; returning placeholder.",
				},
			},
		})
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "patient-chart-service", "status": "ok"})
	})
	r.GET("/metrics", gin.WrapH(promhttp.Handler()))

	_ = r.Run(":8082")
}
