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

- Add retry queue + dead-letter path for failed persistence.
- Emit persistence success/failure events to Kafka.
- Add scheduled reconciliation job and dashboard panels.
