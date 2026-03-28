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
- Data is in-memory (no durable Supabase persistence yet).
- No cryptographic signature verification against clinical signer identities yet.

## Next step

- Integrate a real chain adapter (consortium or L2), store tx metadata in Supabase, and add reconciliation jobs.
