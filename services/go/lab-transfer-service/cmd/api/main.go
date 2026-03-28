package main

import (
	"crypto/rand"
	"encoding/hex"
	"net/http"
	"sync"
	"time"

	"github.com/gin-gonic/gin"
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
)

func newID() (string, error) {
	raw := make([]byte, 12)
	if _, err := rand.Read(raw); err != nil {
		return "", err
	}
	return hex.EncodeToString(raw), nil
}

func main() {
	r := gin.New()
	r.Use(gin.Recovery())

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
		c.JSON(http.StatusOK, transfer)
	})

	r.GET("/healthz", func(c *gin.Context) {
		c.JSON(http.StatusOK, gin.H{"service": "lab-transfer-service", "status": "ok"})
	})

	_ = r.Run(":8084")
}
