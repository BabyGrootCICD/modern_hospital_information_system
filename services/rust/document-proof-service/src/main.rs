use axum::{
    extract::{Path, State},
    routing::{get, post},
    Json, Router,
};
use hex::encode as hex_encode;
use reqwest::Client;
use serde::{Deserialize, Serialize};
use serde_json::json;
use sha2::{Digest, Sha256};
use std::collections::HashMap;
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::RwLock;

#[derive(Clone)]
struct AppState {
    anchors: Arc<RwLock<HashMap<String, AnchorRecord>>>,
    supabase: Option<SupabaseConfig>,
    client: Client,
}

#[derive(Clone)]
struct SupabaseConfig {
    url: String,
    service_key: String,
}

#[derive(Deserialize)]
struct AnchorRequest {
    merkle_root: String,
    network: String,
}

#[derive(Serialize, Deserialize, Clone)]
struct AnchorRecord {
    anchor_id: String,
    merkle_root: String,
    network: String,
    tx_hash: String,
    block_height: u64,
    anchored_at: u64,
}

#[derive(Serialize)]
struct AnchorResponse {
    anchor: AnchorRecord,
    persisted: bool,
}

#[derive(Serialize)]
struct VerifyAnchorResponse {
    found: bool,
    anchor: Option<AnchorRecord>,
}

#[derive(Serialize)]
struct ReconcileResponse {
    key: String,
    supabase_configured: bool,
    persisted: bool,
}

#[derive(Serialize)]
struct Health<'a> {
    service: &'a str,
    status: &'a str,
}

fn now_epoch() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_secs()
}

fn sha256_hex(input: &str) -> String {
    let mut hasher = Sha256::new();
    hasher.update(input.as_bytes());
    hex_encode(hasher.finalize())
}

async fn supabase_insert_anchor(state: &AppState, record: &AnchorRecord) -> bool {
    let Some(cfg) = &state.supabase else {
        return false;
    };

    let endpoint = format!("{}/rest/v1/integrity_events", cfg.url);
    let payload = json!([{
        "event_id": record.anchor_id,
        "aggregate_id": record.anchor_id,
        "aggregate_type": "proof_anchor",
        "digest_sha256": record.merkle_root,
        "merkle_root": record.merkle_root,
        "chain_tx_hash": record.tx_hash
    }]);

    state
        .client
        .post(endpoint)
        .header("apikey", &cfg.service_key)
        .header("Authorization", format!("Bearer {}", cfg.service_key))
        .header("Content-Type", "application/json")
        .header("Prefer", "return=minimal")
        .body(payload.to_string())
        .send()
        .await
        .map(|r| r.status().is_success())
        .unwrap_or(false)
}

async fn supabase_has_anchor(state: &AppState, anchor_id: &str) -> bool {
    let Some(cfg) = &state.supabase else {
        return false;
    };
    let endpoint = format!(
        "{}/rest/v1/integrity_events?event_id=eq.{}&select=event_id",
        cfg.url, anchor_id
    );
    match state
        .client
        .get(endpoint)
        .header("apikey", &cfg.service_key)
        .header("Authorization", format!("Bearer {}", cfg.service_key))
        .send()
        .await
    {
        Ok(resp) if resp.status().is_success() => match resp.text().await {
            Ok(body) => body.contains("event_id"),
            Err(_) => false,
        },
        _ => false,
    }
}

async fn healthz() -> Json<Health<'static>> {
    Json(Health {
        service: "document-proof-service",
        status: "ok",
    })
}

async fn anchor(State(state): State<AppState>, Json(req): Json<AnchorRequest>) -> Json<AnchorResponse> {
    let ts = now_epoch();
    let anchor_id = sha256_hex(&format!("{}:{}:{ts}", req.network, req.merkle_root));
    let tx_hash = sha256_hex(&format!("tx:{}:{}", req.merkle_root, ts));
    let block_height = (ts % 1_000_000) + 10_000;

    let record = AnchorRecord {
        anchor_id: anchor_id.clone(),
        merkle_root: req.merkle_root,
        network: req.network,
        tx_hash,
        block_height,
        anchored_at: ts,
    };

    let mut anchors = state.anchors.write().await;
    anchors.insert(anchor_id, record.clone());
    drop(anchors);

    let persisted = supabase_insert_anchor(&state, &record).await;
    Json(AnchorResponse { anchor: record, persisted })
}

async fn verify_anchor(
    Path(anchor_id): Path<String>,
    State(state): State<AppState>,
) -> Json<VerifyAnchorResponse> {
    let anchors = state.anchors.read().await;
    let found = anchors.get(&anchor_id).cloned();
    Json(VerifyAnchorResponse {
        found: found.is_some(),
        anchor: found,
    })
}

async fn reconcile_anchor(
    Path(anchor_id): Path<String>,
    State(state): State<AppState>,
) -> Json<ReconcileResponse> {
    let persisted = supabase_has_anchor(&state, &anchor_id).await;
    Json(ReconcileResponse {
        key: anchor_id,
        supabase_configured: state.supabase.is_some(),
        persisted,
    })
}

#[tokio::main]
async fn main() {
    let supabase = match (
        std::env::var("SUPABASE_URL").ok(),
        std::env::var("SUPABASE_SERVICE_KEY").ok(),
    ) {
        (Some(url), Some(service_key)) if !url.is_empty() && !service_key.is_empty() => {
            Some(SupabaseConfig { url, service_key })
        }
        _ => None,
    };

    let state = AppState {
        anchors: Arc::new(RwLock::new(HashMap::new())),
        supabase,
        client: Client::new(),
    };

    let app = Router::new()
        .route("/healthz", get(healthz))
        .route("/v1/proofs/anchor", post(anchor))
        .route("/v1/proofs/verify/:anchor_id", get(verify_anchor))
        .route("/v1/proofs/reconcile/:anchor_id", get(reconcile_anchor))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8092")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
