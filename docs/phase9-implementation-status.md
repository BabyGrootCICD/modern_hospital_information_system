# Phase 9 Implementation Status

Status: Implemented for Rust integrity/proof retry hardening.

## Goal

Introduce failure-threshold routing for retry workflows and expose dead-letter visibility APIs.

## Implemented

- `audit-integrity-service`:
  - retry queue entries now track `attempts`
  - configurable threshold via `RETRY_MAX_ATTEMPTS` (default `3`)
  - failed retries routed to dead-letter queue once threshold is reached
  - dead-letter API: `GET /v1/integrity/retry/deadletters`
  - retry status API now reports dead-letter count and max attempts
  - retry run API now reports `moved_to_dead_letter`
- `document-proof-service`:
  - retry queue entries now track `attempts`
  - configurable threshold via `RETRY_MAX_ATTEMPTS` (default `3`)
  - failed retries routed to dead-letter queue once threshold is reached
  - dead-letter API: `GET /v1/proofs/retry/deadletters`
  - retry status API now reports dead-letter count and max attempts
  - retry run API now reports `moved_to_dead_letter`
- Config wiring:
  - `infra/docker-compose.modern.yml` includes `RETRY_MAX_ATTEMPTS` for both Rust services
  - `apps/web-next/.env.example` documents `RETRY_MAX_ATTEMPTS=3`

## Current limits

- Retry and dead-letter queues are still in-memory and reset on service restart.
- No dead-letter replay endpoint yet.
- No Kafka events for retry/dead-letter transitions yet.

## Next step

- Persist retry/dead-letter state in Redis/Supabase.
- Add controlled dead-letter replay endpoint.
- Emit retry/dead-letter lifecycle events to Kafka and add Grafana panels.
