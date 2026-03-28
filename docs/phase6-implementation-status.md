# Phase 6 Implementation Status

Status: Integrity/blockchain pilot APIs implemented with simulated anchoring.

## Implemented

- Rust `audit-integrity-service`:
  - create integrity event (`/v1/integrity/events`)
  - list events by aggregate (`/v1/integrity/events/:aggregate_id`)
  - verify hash chain (`/v1/integrity/verify/:aggregate_id`)
  - compute Merkle root (`/v1/integrity/merkle-root`)
- Rust `document-proof-service`:
  - anchor proof (`/v1/proofs/anchor`)
  - verify anchor (`/v1/proofs/verify/:anchor_id`)
- Next.js BFF routes:
  - `/api/bff/integrity/events`
  - `/api/bff/integrity/verify/:aggregateID`
  - `/api/bff/proofs/anchor`
- Nginx routes:
  - `/api/integrity/*`
  - `/api/proofs/*`

## Current limits

- Anchoring is currently simulated (no live blockchain transaction submission).
- Durable Supabase persistence is now optional and environment-driven.
- No cryptographic signature verification against clinical signer identities yet.

## Progress since kickoff

- Added Supabase persistence hooks in both Rust services (`SUPABASE_URL`, `SUPABASE_SERVICE_KEY`).
- Added reconcile endpoints to check whether integrity/proof records are persisted.
- Added retry queue status and retry run endpoints plus background retry workers.
- Added dead-letter handling and dead-letter inspection endpoints with `RETRY_MAX_ATTEMPTS` threshold control.
- Added Redis-backed retry/dead-letter state persistence and dead-letter replay endpoints.

## Next step

- Integrate a real chain adapter (consortium or L2), store tx metadata in Supabase, and add reconciliation jobs.
