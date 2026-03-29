# Phase 6 Implementation Status

Status: Integrity/blockchain pilot APIs implemented with external-adapter + finality baseline.

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

- External adapter mode exists, but production chain endpoint reliability and finality SLAs are still pending.
- Durable Supabase persistence is now optional and environment-driven.
- No cryptographic signature verification against clinical signer identities yet.

## Progress since kickoff

- Added Supabase persistence hooks in both Rust services (`SUPABASE_URL`, `SUPABASE_SERVICE_KEY`).
- Added reconcile endpoints to check whether integrity/proof records are persisted.
- Added retry queue status and retry run endpoints plus background retry workers.
- Added dead-letter handling and dead-letter inspection endpoints with `RETRY_MAX_ATTEMPTS` threshold control.
- Added Redis-backed retry/dead-letter state persistence and dead-letter replay endpoints.
- Added legal report endpoint (`/v1/integrity/legal-report/:aggregate_id`) for audit export baseline.
- Added chain adapter/finality config path in proof service (`CHAIN_ADAPTER_URL`, `CHAIN_STATUS_URL`, retry/backoff envs).

## Next step

- Complete production adapter contracts, finality incident policies, and signer identity verification.

## Potential TODOs

- Add deterministic report export format (PDF + signed JSON bundle).
- Add block explorer deep-link metadata for each anchored transaction.
