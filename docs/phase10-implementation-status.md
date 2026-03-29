# Phase 10 Implementation Status

Status: Implemented for Redis-backed durability and dead-letter replay in Rust integrity/proof services.

## Goal

Persist retry/dead-letter workflow state across restarts and provide operator replay controls.

## Implemented

- `audit-integrity-service`:
  - Redis state load on startup for retry/dead-letter queues
  - Redis state persistence on retry queue/dead-letter mutations
  - dead-letter replay endpoint: `POST /v1/integrity/retry/replay-deadletters`
  - existing retry/dead-letter endpoints retained
- `document-proof-service`:
  - Redis state load on startup for retry/dead-letter queues
  - Redis state persistence on retry queue/dead-letter mutations
  - dead-letter replay endpoint: `POST /v1/proofs/retry/replay-deadletters`
  - existing retry/dead-letter endpoints retained
- Runtime/config:
  - both services now read `REDIS_URL`
  - compose injects `REDIS_URL` with default `redis://redis:6379`
  - Rust service dependencies now include async `redis` crate

## Verification

- `cargo check` passed for:
  - `services/rust/audit-integrity-service`
  - `services/rust/document-proof-service`

## Current limits

- Redis usage is queue snapshot persistence (JSON blobs), not coordinated streaming/consumer-group queue semantics.
- No Kafka retry/dead-letter events yet.
- No authz/audit controls on replay endpoints yet.

## Next step

- Emit retry/dead-letter lifecycle events into Kafka topics.
- Add replay authorization and replay audit events.
- Move from snapshot persistence to a coordinated queue model (Redis lists/streams or Supabase-backed job table).
- Containerize all Go/Rust services with production Dockerfiles and build-based compose deployment.

## Potential TODOs

- Add queue backpressure controls and high-watermark alerts.
- Add retry policy profiles per event type and service criticality.
