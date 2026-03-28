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
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::RwLock;

#[derive(Clone)]
struct AppState {
    events: Arc<RwLock<Vec<IntegrityEvent>>>,
    retry_queue: Arc<RwLock<Vec<RetryItem>>>,
    dead_letters: Arc<RwLock<Vec<RetryItem>>>,
    retry_max_attempts: u8,
    redis_url: Option<String>,
    supabase: Option<SupabaseConfig>,
    client: Client,
}

#[derive(Clone)]
struct SupabaseConfig {
    url: String,
    service_key: String,
}

#[derive(Serialize, Deserialize, Clone)]
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
struct CreateEventResponse {
    event: IntegrityEvent,
    persisted: bool,
}

#[derive(Serialize, Deserialize, Clone)]
struct RetryItem {
    event: IntegrityEvent,
    attempts: u8,
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

const REDIS_RETRY_QUEUE_KEY: &str = "audit_integrity:retry_queue";
const REDIS_DEAD_LETTER_KEY: &str = "audit_integrity:dead_letters";

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

async fn supabase_insert_event(state: &AppState, event: &IntegrityEvent) -> bool {
    let Some(cfg) = &state.supabase else {
        return false;
    };

    let endpoint = format!("{}/rest/v1/integrity_events", cfg.url);
    let payload = json!([{
        "event_id": event.event_id,
        "aggregate_id": event.aggregate_id,
        "aggregate_type": event.aggregate_type,
        "digest_sha256": event.payload_hash,
        "merkle_root": serde_json::Value::Null,
        "chain_tx_hash": serde_json::Value::Null
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

async fn supabase_has_event(state: &AppState, event_id: &str) -> bool {
    let Some(cfg) = &state.supabase else {
        return false;
    };
    let endpoint = format!(
        "{}/rest/v1/integrity_events?event_id=eq.{}&select=event_id",
        cfg.url, event_id
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
        service: "audit-integrity-service",
        status: "ok",
    })
}

async fn create_event(
    State(state): State<AppState>,
    Json(req): Json<CreateEventRequest>,
) -> Json<CreateEventResponse> {
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
    drop(events);

    let persisted = supabase_insert_event(&state, &event).await;
    if !persisted && state.supabase.is_some() {
        let mut queue = state.retry_queue.write().await;
        queue.push(RetryItem {
            event: event.clone(),
            attempts: 1,
        });
        drop(queue);
        persist_retry_state_to_redis(&state).await;
    }
    Json(CreateEventResponse { event, persisted })
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

async fn reconcile(Path(event_id): Path<String>, State(state): State<AppState>) -> Json<ReconcileResponse> {
    let persisted = supabase_has_event(&state, &event_id).await;
    Json(ReconcileResponse {
        key: event_id,
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
        if supabase_insert_event(state, &item.event).await {
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
        events: Arc::new(RwLock::new(Vec::new())),
        retry_queue: Arc::new(RwLock::new(Vec::new())),
        dead_letters: Arc::new(RwLock::new(Vec::new())),
        retry_max_attempts: std::env::var("RETRY_MAX_ATTEMPTS")
            .ok()
            .and_then(|v| v.parse::<u8>().ok())
            .unwrap_or(3),
        redis_url: std::env::var("REDIS_URL")
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
        .route("/v1/integrity/events", post(create_event))
        .route("/v1/integrity/events/:aggregate_id", get(list_events))
        .route("/v1/integrity/verify/:aggregate_id", get(verify_events))
        .route("/v1/integrity/merkle-root", get(merkle))
        .route("/v1/integrity/reconcile/:event_id", get(reconcile))
        .route("/v1/integrity/retry/status", get(retry_status))
        .route("/v1/integrity/retry/run", post(retry_run))
        .route("/v1/integrity/retry/deadletters", get(dead_letters))
        .route("/v1/integrity/retry/replay-deadletters", post(replay_dead_letters))
        .with_state(state);

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8091")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
