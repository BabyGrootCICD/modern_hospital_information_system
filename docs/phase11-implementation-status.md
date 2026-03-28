# Phase 11 Implementation Status

Status: Implemented for container build/deploy baseline of all Go/Rust backend services.

## Goal

Standardize service packaging with Dockerfiles and provide a compose stack that builds runnable images directly from source.

## Implemented

- Dockerfiles added for all Go services:
  - `services/go/identity-access-service/Dockerfile`
  - `services/go/patient-chart-service/Dockerfile`
  - `services/go/order-clinical-service/Dockerfile`
  - `services/go/lab-transfer-service/Dockerfile`
  - `services/go/sync-gateway-service/Dockerfile`
- Dockerfiles added for all Rust services:
  - `services/rust/audit-integrity-service/Dockerfile`
  - `services/rust/document-proof-service/Dockerfile`
- New compose stack:
  - `infra/docker-compose.services.yml`
  - builds each Go/Rust service image via `build.context` + Dockerfile
  - includes runtime dependencies: `redis`, `zookeeper`, `kafka`, `otel-collector`
  - maps service ports for all backend APIs

## Current limits

- Compose stack has no per-service healthcheck policies yet.
- No image publishing/signing workflow yet.
- No CI step validates compose build for all service images.

## Next step

- Add healthchecks and startup dependency conditions.
- Add CI pipeline jobs for Docker build, vulnerability scanning, and image push.
- Add versioned image tags and release promotion rules.
- Complete Redis/Supabase durability + outbox/idempotent consumer layers and attach Robot e2e coverage.
