use axum::{
    extract::{Path, State},
    routing::{get, post},
    Json, Router,
};
use hex::encode as hex_encode;
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::collections::HashMap;
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::RwLock;

#[derive(Clone)]
struct AppState {
    anchors: Arc<RwLock<HashMap<String, AnchorRecord>>>,
}

#[derive(Deserialize)]
struct AnchorRequest {
    merkle_root: String,
    network: String,
}

#[derive(Serialize, Clone)]
struct AnchorRecord {
    anchor_id: String,
    merkle_root: String,
    network: String,
    tx_hash: String,
    block_height: u64,
    anchored_at: u64,
}

#[derive(Serialize)]
struct VerifyAnchorResponse {
    found: bool,
    anchor: Option<AnchorRecord>,
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

async fn healthz() -> Json<Health<'static>> {
    Json(Health {
        service: "document-proof-service",
        status: "ok",
    })
}

async fn anchor(State(state): State<AppState>, Json(req): Json<AnchorRequest>) -> Json<AnchorRecord> {
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
    Json(record)
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

#[tokio::main]
async fn main() {
    let state = AppState {
        anchors: Arc::new(RwLock::new(HashMap::new())),
    };

    let app = Router::new()
        .route("/healthz", get(healthz))
        .route("/v1/proofs/anchor", post(anchor))
        .route("/v1/proofs/verify/:anchor_id", get(verify_anchor))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8092")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
