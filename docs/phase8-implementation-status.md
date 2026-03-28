# Phase 8 Implementation Status

Status: Reliability hardening implemented for integrity/proof persistence retries; Phase 9 threshold routing is active.

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

## Current limits

- Retry queues are process-memory only (lost on restart).
- Dead-letter queues are process-memory only (lost on restart).
- No Kafka event emission for retry/dead-letter outcomes yet.

## Next step

- Move retry and dead-letter queue state to Redis/Supabase tables.
- Emit retry success/failure/dead-letter events to Kafka.
- Add dead-letter replay endpoint with operator authorization.
