use axum::{
    extract::{Path, State},
    routing::{get, post},
    Json, Router,
};
use hex::encode as hex_encode;
use redis::AsyncCommands;
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
    retry_queue: Arc<RwLock<Vec<RetryItem>>>,
    dead_letters: Arc<RwLock<Vec<RetryItem>>>,
    retry_max_attempts: u8,
    redis_url: Option<String>,
    chain_adapter_url: Option<String>,
    chain_adapter_token: Option<String>,
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
    adapter_mode: String,
}

#[derive(Deserialize)]
struct ChainAnchorAdapterResponse {
    tx_hash: String,
    block_height: u64,
}

#[derive(Serialize, Deserialize, Clone)]
struct RetryItem {
    anchor: AnchorRecord,
    attempts: u8,
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
struct RetryStatusResponse {
    queued: usize,
    dead_letters: usize,
    max_attempts: u8,
    supabase_configured: bool,
}

#[derive(Serialize)]
struct RetryRunResponse {
    attempted: usize,
    succeeded: usize,
    failed: usize,
    moved_to_dead_letter: usize,
}

#[derive(Serialize)]
struct DeadLetterReplayResponse {
    moved: usize,
    queued: usize,
    dead_letters: usize,
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

const REDIS_RETRY_QUEUE_KEY: &str = "document_proof:retry_queue";
const REDIS_DEAD_LETTER_KEY: &str = "document_proof:dead_letters";

async fn load_retry_state_from_redis(state: &AppState) {
    let Some(redis_url) = &state.redis_url else {
        return;
    };
    let Ok(client) = redis::Client::open(redis_url.as_str()) else {
        return;
    };
    let Ok(mut conn) = client.get_multiplexed_async_connection().await else {
        return;
    };

    let queued_raw: Option<String> = conn.get(REDIS_RETRY_QUEUE_KEY).await.ok();
    let dead_raw: Option<String> = conn.get(REDIS_DEAD_LETTER_KEY).await.ok();

    if let Some(raw) = queued_raw {
        if let Ok(items) = serde_json::from_str::<Vec<RetryItem>>(&raw) {
            let mut queue = state.retry_queue.write().await;
            *queue = items;
        }
    }
    if let Some(raw) = dead_raw {
        if let Ok(items) = serde_json::from_str::<Vec<RetryItem>>(&raw) {
            let mut dlq = state.dead_letters.write().await;
            *dlq = items;
        }
    }
}

async fn persist_retry_state_to_redis(state: &AppState) {
    let Some(redis_url) = &state.redis_url else {
        return;
    };
    let Ok(client) = redis::Client::open(redis_url.as_str()) else {
        return;
    };
    let Ok(mut conn) = client.get_multiplexed_async_connection().await else {
        return;
    };

    let queued = state.retry_queue.read().await.clone();
    let dead = state.dead_letters.read().await.clone();
    let queued_json = match serde_json::to_string(&queued) {
        Ok(v) => v,
        Err(_) => return,
    };
    let dead_json = match serde_json::to_string(&dead) {
        Ok(v) => v,
        Err(_) => return,
    };

    let _set_queue: redis::RedisResult<()> = conn.set(REDIS_RETRY_QUEUE_KEY, queued_json).await;
    let _set_dead: redis::RedisResult<()> = conn.set(REDIS_DEAD_LETTER_KEY, dead_json).await;
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

async fn submit_chain_anchor(state: &AppState, merkle_root: &str, network: &str) -> (String, u64, String) {
    let ts = now_epoch();
    if let Some(url) = &state.chain_adapter_url {
        let payload = json!({
            "merkle_root": merkle_root,
            "network": network,
            "anchored_at": ts
        });
        let mut req = state.client.post(url).json(&payload);
        if let Some(token) = &state.chain_adapter_token {
            req = req.bearer_auth(token);
        }
        if let Ok(resp) = req.send().await {
            if resp.status().is_success() {
                if let Ok(parsed) = resp.json::<ChainAnchorAdapterResponse>().await {
                    return (parsed.tx_hash, parsed.block_height, "external-adapter".to_string());
                }
            }
        }
    }
    let tx_hash = sha256_hex(&format!("tx:{}:{}", merkle_root, ts));
    let block_height = (ts % 1_000_000) + 10_000;
    (tx_hash, block_height, "simulated".to_string())
}

async fn anchor(State(state): State<AppState>, Json(req): Json<AnchorRequest>) -> Json<AnchorResponse> {
    let ts = now_epoch();
    let anchor_id = sha256_hex(&format!("{}:{}:{ts}", req.network, req.merkle_root));
    let (tx_hash, block_height, adapter_mode) =
        submit_chain_anchor(&state, &req.merkle_root, &req.network).await;

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
    if !persisted && state.supabase.is_some() {
        let mut queue = state.retry_queue.write().await;
        queue.push(RetryItem {
            anchor: record.clone(),
            attempts: 1,
        });
        drop(queue);
        persist_retry_state_to_redis(&state).await;
    }
    Json(AnchorResponse {
        anchor: record,
        persisted,
        adapter_mode,
    })
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

async fn retry_status(State(state): State<AppState>) -> Json<RetryStatusResponse> {
    let queue = state.retry_queue.read().await;
    let dead_letters = state.dead_letters.read().await;
    Json(RetryStatusResponse {
        queued: queue.len(),
        dead_letters: dead_letters.len(),
        max_attempts: state.retry_max_attempts,
        supabase_configured: state.supabase.is_some(),
    })
}

async fn run_retry_once(state: &AppState) -> RetryRunResponse {
    let pending = {
        let mut queue = state.retry_queue.write().await;
        std::mem::take(&mut *queue)
    };

    let mut succeeded = 0usize;
    let mut failed_items = Vec::new();
    let mut dead_items = Vec::new();
    for item in pending {
        if supabase_insert_anchor(state, &item.anchor).await {
            succeeded += 1;
        } else {
            let mut next = item.clone();
            next.attempts = next.attempts.saturating_add(1);
            if next.attempts >= state.retry_max_attempts {
                dead_items.push(next);
            } else {
                failed_items.push(next);
            }
        }
    }

    let failed = failed_items.len();
    let moved_to_dead_letter = dead_items.len();
    {
        let mut queue = state.retry_queue.write().await;
        queue.extend(failed_items);
    }
    if moved_to_dead_letter > 0 {
        let mut dlq = state.dead_letters.write().await;
        dlq.extend(dead_items);
    }

    let result = RetryRunResponse {
        attempted: succeeded + failed + moved_to_dead_letter,
        succeeded,
        failed,
        moved_to_dead_letter,
    };
    persist_retry_state_to_redis(state).await;
    result
}

async fn retry_run(State(state): State<AppState>) -> Json<RetryRunResponse> {
    Json(run_retry_once(&state).await)
}

async fn dead_letters(State(state): State<AppState>) -> Json<Vec<RetryItem>> {
    let dlq = state.dead_letters.read().await;
    Json(dlq.clone())
}

async fn replay_dead_letters(State(state): State<AppState>) -> Json<DeadLetterReplayResponse> {
    let moved_items = {
        let mut dlq = state.dead_letters.write().await;
        std::mem::take(&mut *dlq)
    };
    let moved = moved_items.len();
    if moved > 0 {
        let mut queue = state.retry_queue.write().await;
        queue.extend(moved_items);
    }
    persist_retry_state_to_redis(&state).await;
    let queued = state.retry_queue.read().await.len();
    let dead_letters = state.dead_letters.read().await.len();
    Json(DeadLetterReplayResponse {
        moved,
        queued,
        dead_letters,
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
        retry_queue: Arc::new(RwLock::new(Vec::new())),
        dead_letters: Arc::new(RwLock::new(Vec::new())),
        retry_max_attempts: std::env::var("RETRY_MAX_ATTEMPTS")
            .ok()
            .and_then(|v| v.parse::<u8>().ok())
            .unwrap_or(3),
        redis_url: std::env::var("REDIS_URL")
            .ok()
            .filter(|v| !v.trim().is_empty()),
        chain_adapter_url: std::env::var("CHAIN_ADAPTER_URL")
            .ok()
            .filter(|v| !v.trim().is_empty()),
        chain_adapter_token: std::env::var("CHAIN_ADAPTER_TOKEN")
            .ok()
            .filter(|v| !v.trim().is_empty()),
        supabase,
        client: Client::new(),
    };
    load_retry_state_from_redis(&state).await;

    let worker_state = state.clone();
    tokio::spawn(async move {
        let mut interval = tokio::time::interval(std::time::Duration::from_secs(30));
        loop {
            interval.tick().await;
            if worker_state.supabase.is_some() {
                let _ = run_retry_once(&worker_state).await;
            }
        }
    });

    let app = Router::new()
        .route("/healthz", get(healthz))
        .route("/v1/proofs/anchor", post(anchor))
        .route("/v1/proofs/verify/:anchor_id", get(verify_anchor))
        .route("/v1/proofs/reconcile/:anchor_id", get(reconcile_anchor))
        .route("/v1/proofs/retry/status", get(retry_status))
        .route("/v1/proofs/retry/run", post(retry_run))
        .route("/v1/proofs/retry/deadletters", get(dead_letters))
        .route("/v1/proofs/retry/replay-deadletters", post(replay_dead_letters))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8092")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
