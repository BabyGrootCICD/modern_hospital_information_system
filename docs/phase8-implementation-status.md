# Phase 8 Implementation Status

Status: Reliability hardening implemented for integrity/proof persistence retries; Phase 9 threshold routing is active.
Phase 10 note: retry/dead-letter state is now persisted to Redis with replay endpoints.

## Implemented

- `audit-integrity-service`:
  - retry queue for failed Supabase writes
  - background retry worker (30s interval)
  - retry status endpoint: `/v1/integrity/retry/status`
  - manual retry endpoint: `/v1/integrity/retry/run`
  - dead-letter inspection endpoint: `/v1/integrity/retry/deadletters`
- `document-proof-service`:
  - retry queue for failed Supabase writes
  - background retry worker (30s interval)
  - retry status endpoint: `/v1/proofs/retry/status`
  - manual retry endpoint: `/v1/proofs/retry/run`
  - dead-letter inspection endpoint: `/v1/proofs/retry/deadletters`
- Failure threshold routing:
  - `RETRY_MAX_ATTEMPTS` env support in both Rust services (default `3`)
  - retries beyond threshold move into dead-letter queue
- Durability and replay:
  - retry/dead-letter snapshots are persisted to Redis
  - startup restores retry/dead-letter state from Redis
  - replay endpoints move dead-letter records back to retry queue

## Current limits

- Redis state persistence is snapshot-style per service process and not yet multi-consumer coordinated.
- No Kafka event emission for retry/dead-letter outcomes yet.

## Next step

- Emit retry success/failure/dead-letter events to Kafka.
- Add dead-letter replay authorization controls and operator audit events.

## Potential TODOs

- Add dead-letter replay rate limiting and replay window constraints.
- Add per-tenant dead-letter retention policies.
