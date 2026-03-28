# Phase 2 Implementation Status

Status: Core extraction APIs implemented; persistence/event layers pending.

## Goal

Extract backend capabilities from legacy monolith boundaries into independently deployable microservices.

## Implemented

- `identity-access-service`:
  - token issuing endpoint
  - token validation endpoint
  - role policy endpoint
- `lab-transfer-service`:
  - create/get/complete transfer workflow
- `sync-gateway-service`:
  - create/start/complete/get sync job workflow
- Strangler ingress routing:
  - `/api/identity/*`
  - `/api/lab/*`
  - `/api/sync/*`
- Local compose services for all three Go APIs.
- Next.js BFF endpoint for sync job creation: `/api/bff/sync/jobs`.
- Supabase-oriented chart retrieval path via `patient-chart-service` and `/api/bff/charts/:patientID`.
- Legacy .NET backend code removed from repository.

## Current limits

- In-memory storage only (no Redis/Kafka/Supabase persistence yet).
- No service-to-service auth tokens yet.
- Kafka publish hooks now exist, but outbox + durable consumer flows are still pending.
- Metrics endpoints now exist, but SLO alerting and trace correlation are pending.
- Integrity verification APIs are now available, but service-to-service auth on those paths is still pending.
- Integrity/proof persistence now supports Supabase hooks, but cross-service transaction consistency is pending.
- Integrity/proof retry behavior now exists, but durable queue storage is pending.

## Next incremental step

- Add Redis-backed job/session storage and Kafka publish hooks for transfer/sync state transitions.
- Enforce service JWT validation for all write endpoints.
- Add idempotency keys and request replay safety for transfer/sync create endpoints.
