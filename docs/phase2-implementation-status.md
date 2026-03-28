# Phase 2 Implementation Status

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

## Current limits

- In-memory storage only (no Redis/Kafka/Supabase persistence yet).
- No service-to-service auth tokens yet.
- No outbox/event publishing yet.

## Next incremental step

- Add Redis-backed job/session storage and Kafka publish hooks for transfer/sync state transitions.
