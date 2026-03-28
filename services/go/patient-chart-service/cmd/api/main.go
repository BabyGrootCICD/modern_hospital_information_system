package main

import (
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/url"
	"os"

	"github.com/gin-gonic/gin"
)

type PatientChart struct {
	PatientID  string `json:"patient_id"`
	VisitDate  string `json:"visit_date"`
	Department string `json:"department"`
	DoctorID   string `json:"doctor_id"`
	Summary    string `json:"summary"`
}

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

func main() {
	r := gin.New()
	r.Use(gin.Recovery())

	r.GET("/v1/charts/:patientID", func(c *gin.Context) {
		patientID := c.Param("patientID")
		charts, err := fetchFromSupabase(patientID)
		if err == nil {
			c.JSON(http.StatusOK, gin.H{"source": "supabase", "items": charts})
			return
		}

		c.JSON(http.StatusOK, gin.H{
			"source": "fallback-mock",
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

	_ = r.Run(":8082")
}
