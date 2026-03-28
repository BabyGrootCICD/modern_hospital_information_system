# Phase 7 Implementation Status

Status: Durability hardening in progress for integrity/proof services.

## Implemented

- `audit-integrity-service`:
  - optional Supabase insert on event creation
  - reconcile endpoint: `/v1/integrity/reconcile/:event_id`
- `document-proof-service`:
  - optional Supabase insert on anchor creation
  - reconcile endpoint: `/v1/proofs/reconcile/:anchor_id`
- Compose wiring:
  - `SUPABASE_URL` and `SUPABASE_SERVICE_KEY` injected into both Rust services

## Current limits

- Persistence uses best-effort insert (no retries/backoff queue yet).
- No reconciliation scheduler/job yet; only on-demand API checks.
- No strict cross-service transactional guarantee between event creation and persistence.

## Next step

- Emit persistence success/failure and dead-letter events to Kafka.
- Add scheduled reconciliation job and dashboard panels.
- Migrate retry/dead-letter state from memory to Redis/Supabase.

## Progress update

- Retry queue and background retry worker implemented.
- Manual retry trigger and queue status endpoints implemented.
- Dead-letter handling and dead-letter inspection endpoints implemented.
- Kafka persistence/dead-letter events remain pending.
- Redis-backed retry/dead-letter durability and replay APIs implemented.
