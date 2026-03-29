# Phase 2 Implementation Status

Status: Core extraction APIs implemented; persistence/event layers in active hardening.

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

- Redis-backed durability is implemented for mutable Go services, but Supabase canonical write-through remains partial.
- Service-to-service JWT validation is implemented in Go APIs, but key rotation automation is still pending.
- Kafka outbox + idempotent consumer baseline exists, but broader topic coverage is still pending.
- Metrics/alerts stack is wired, but tenant-segmented SLO dashboards remain pending.

## Next incremental step

- Add Redis-backed job/session storage and Kafka publish hooks for transfer/sync state transitions.
- Enforce service JWT validation for all write endpoints.
- Add idempotency keys and request replay safety for transfer/sync create endpoints.

## Potential TODOs

- Add OpenAPI contract tests for identity/lab/sync endpoints in CI.
- Add token audience/issuer claim enforcement for internal JWTs.
