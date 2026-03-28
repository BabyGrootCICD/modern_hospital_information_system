package main

import (
	"crypto/rand"
	"encoding/hex"
	"net/http"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
)

type CreateSyncJobRequest struct {
	Mode      string `json:"mode"`
	Hospital  string `json:"hospital"`
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
)

func randomID() (string, error) {
	raw := make([]byte, 12)
	if _, err := rand.Read(raw); err != nil {
		return "", err
	}
	return hex.EncodeToString(raw), nil
}

func main() {
	r := gin.New()
	r.Use(gin.Recovery())

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

	_ = r.Run(":8085")
}
