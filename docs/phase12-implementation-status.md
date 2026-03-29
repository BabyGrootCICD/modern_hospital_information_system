# Phase 12 Implementation Status

Status: Implemented baseline across durability, eventing, auth/tenant propagation, observability, blockchain adapter/reporting, CI hardening, and Robot e2e suite.

## Implemented

- Go durability upgrades:
  - `identity-access-service`, `lab-transfer-service`, `sync-gateway-service` now persist runtime state snapshots into Redis.
- Kafka outbox + replay/dead-letter:
  - outbox pending/dead-letter/processed tracking added in Go mutable services.
  - replay endpoints added for outbox dead-letters.
  - `sync-gateway-service` now has idempotent consumer baseline for `sync.commands` with consumer dead-letter + replay endpoint.
- Service-to-service auth and tenant propagation:
  - internal JWT middleware (HS256 secret via `INTERNAL_JWT_SECRET`) added across all Go APIs (`identity`, `patient-chart`, `order-clinical`, `lab-transfer`, `sync-gateway`), with health/metrics exceptions.
  - tenant propagated via `X-Tenant-ID` / JWT claim into stored records and event headers/payloads.
- Observability completion baseline:
  - Prometheus alert rules added.
  - Loki and Tempo configs/services added.
  - OTEL collector exports traces to Tempo.
  - Grafana datasources and dashboard provisioning added with backend overview dashboard.
- Blockchain productionization baseline:
  - `document-proof-service` supports external chain adapter mode via `CHAIN_ADAPTER_URL` and `CHAIN_ADAPTER_TOKEN`, with simulated fallback.
  - `audit-integrity-service` adds legal-style report endpoint: `GET /v1/integrity/legal-report/:aggregate_id`.
- Container runtime/CI hardening:
  - healthchecks added in compose stacks.
  - CI adds container build+scan+sign+publish/promote workflow stages.
  - CI includes Robot full suite execution and artifact upload.
  - dedicated workflows added:
    - backend CI (`.github/workflows/backend-ci.yml`) builds all backends and runs all suites.
    - frontend+infra CI (`.github/workflows/frontend-infra-ci.yml`) validates frontend build and infra compose/config.
  - CI stabilization updates:
    - actions forced to Node 24 runtime (`FORCE_JAVASCRIPT_ACTIONS_TO_NODE24=true`) to avoid Node 20 deprecation cutoff risk.
    - Go cache dependency path set per service (`services/go/<service>/go.sum`) to avoid cache restore path errors.
    - backend report artifact upload hardened (`test/report/.keep`, `if-no-files-found: warn`).
- Robot Framework full suite:
  - full test suite created in `test/suites` with health/auth/outbox/blockchain coverage.
  - report output path standardized to `test/report`.

## Remaining

- Supabase table-backed persistence for the newly Redis-backed Go service state is still pending.
- Consumer coverage for additional topics is pending.
- Production-grade blockchain finality verification and legal workflow sign-off remain pending.

## Potential TODOs

- Add CI flake-detection and automatic retry policy for integration-test suites.
- Add a synthetic canary test stage post-deploy using Robot smoke packs.
