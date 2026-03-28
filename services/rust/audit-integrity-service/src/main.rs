use axum::{
    extract::{Path, State},
    routing::{get, post},
    Json, Router,
};
use hex::encode as hex_encode;
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::RwLock;

#[derive(Clone)]
struct AppState {
    events: Arc<RwLock<Vec<IntegrityEvent>>>,
}

#[derive(Serialize, Clone)]
struct IntegrityEvent {
    event_id: String,
    aggregate_id: String,
    aggregate_type: String,
    payload_hash: String,
    chain_hash: String,
    created_at: u64,
}

#[derive(Deserialize)]
struct CreateEventRequest {
    aggregate_id: String,
    aggregate_type: String,
    payload: String,
}

#[derive(Serialize)]
struct VerifyResponse {
    aggregate_id: String,
    chain_valid: bool,
    event_count: usize,
    latest_chain_hash: Option<String>,
}

#[derive(Serialize)]
struct MerkleResponse {
    root_hash: String,
    leaf_count: usize,
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

fn merkle_root(mut leaves: Vec<String>) -> String {
    if leaves.is_empty() {
        return sha256_hex("");
    }
    while leaves.len() > 1 {
        let mut next = Vec::new();
        for pair in leaves.chunks(2) {
            let left = &pair[0];
            let right = if pair.len() == 2 { &pair[1] } else { &pair[0] };
            next.push(sha256_hex(&format!("{left}{right}")));
        }
        leaves = next;
    }
    leaves.remove(0)
}

async fn healthz() -> Json<Health<'static>> {
    Json(Health {
        service: "audit-integrity-service",
        status: "ok",
    })
}

async fn create_event(
    State(state): State<AppState>,
    Json(req): Json<CreateEventRequest>,
) -> Json<IntegrityEvent> {
    let payload_hash = sha256_hex(&req.payload);
    let ts = now_epoch();

    let mut events = state.events.write().await;
    let last_chain_hash = events
        .iter()
        .rev()
        .find(|e| e.aggregate_id == req.aggregate_id)
        .map(|e| e.chain_hash.clone())
        .unwrap_or_else(|| sha256_hex("genesis"));

    let chain_hash = sha256_hex(&format!("{last_chain_hash}{payload_hash}{ts}"));
    let event = IntegrityEvent {
        event_id: sha256_hex(&format!("{}:{}:{ts}", req.aggregate_id, payload_hash)),
        aggregate_id: req.aggregate_id,
        aggregate_type: req.aggregate_type,
        payload_hash,
        chain_hash,
        created_at: ts,
    };
    events.push(event.clone());
    Json(event)
}

async fn list_events(
    Path(aggregate_id): Path<String>,
    State(state): State<AppState>,
) -> Json<Vec<IntegrityEvent>> {
    let events = state.events.read().await;
    let filtered = events
        .iter()
        .filter(|x| x.aggregate_id == aggregate_id)
        .cloned()
        .collect::<Vec<_>>();
    Json(filtered)
}

async fn verify_events(
    Path(aggregate_id): Path<String>,
    State(state): State<AppState>,
) -> Json<VerifyResponse> {
    let events = state.events.read().await;
    let filtered = events
        .iter()
        .filter(|x| x.aggregate_id == aggregate_id)
        .cloned()
        .collect::<Vec<_>>();

    let mut previous = sha256_hex("genesis");
    let mut chain_valid = true;
    for e in &filtered {
        let recomputed = sha256_hex(&format!("{previous}{}{}", e.payload_hash, e.created_at));
        if recomputed != e.chain_hash {
            chain_valid = false;
            break;
        }
        previous = e.chain_hash.clone();
    }

    Json(VerifyResponse {
        aggregate_id,
        chain_valid,
        event_count: filtered.len(),
        latest_chain_hash: filtered.last().map(|x| x.chain_hash.clone()),
    })
}

async fn merkle(State(state): State<AppState>) -> Json<MerkleResponse> {
    let events = state.events.read().await;
    let leaves = events.iter().map(|e| e.chain_hash.clone()).collect::<Vec<_>>();
    Json(MerkleResponse {
        root_hash: merkle_root(leaves.clone()),
        leaf_count: leaves.len(),
    })
}

#[tokio::main]
async fn main() {
    let state = AppState {
        events: Arc::new(RwLock::new(Vec::new())),
    };

    let app = Router::new()
        .route("/healthz", get(healthz))
        .route("/v1/integrity/events", post(create_event))
        .route("/v1/integrity/events/:aggregate_id", get(list_events))
        .route("/v1/integrity/verify/:aggregate_id", get(verify_events))
        .route("/v1/integrity/merkle-root", get(merkle))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8091")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
