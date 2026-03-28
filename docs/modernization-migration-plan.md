# SHIS Modernization Migration Plan

## 0) Live Progress Snapshot (2026-03-28)

- Completed:
  - Phase 0 foundation scaffolding (Next.js, Go, Rust, infra baseline).
  - Phase 1 frontend shell (locale routing, session auth scaffold, protected pages, BFF endpoints, Nginx routes).
  - Phase 2 backend kickoff (identity, lab-transfer, sync-gateway APIs and ingress routes).
  - Phase 3 kickoff (Supabase-oriented patient-chart service integration and SQL migration baseline).
  - Legacy .NET backend projects removed from repository and delivery path.
  - Phase 4 kickoff (Kafka event emission hooks in Go services).
  - Phase 5 kickoff (Prometheus metrics endpoints across Go services + Prometheus/OTEL stack wiring).
  - Integrity pilot kickoff (Rust digest chain + Merkle root + proof anchor APIs).
  - Phase 7 kickoff (Supabase persistence hooks and reconciliation endpoints for Rust integrity/proof services).
  - Phase 8 kickoff (retry queues + background reconciliation workers for failed integrity/proof persistence).
  - Phase 9 kickoff (failure-threshold dead-letter handling for integrity/proof retry flows).
- In progress:
  - Service persistence hardening (move in-memory stores to Supabase/Redis).
  - Contract stabilization across BFF and microservices.
- Not started:
  - Kafka outbox/event stream consumers and replay handling.
  - full observability SLO dashboards and alert policies.
  - blockchain network adapter for real chain transactions (current anchor remains simulated).

## 1) Current-State Scan Summary

### Existing solution layout
- Legacy .NET projects were present originally but are now removed from repository.
- Active backend stack is Go-Gin microservices + Rust services.
- SQL snapshots/dumps at repository root remain as migration references.

### Key migration implications
- Domain logic originated in the legacy monolith and still requires domain-by-domain parity validation.
- Offline synchronization remains business-critical and is now modeled in `sync-gateway-service`.
- Existing PostgreSQL usage lowers migration risk to Supabase (also PostgreSQL), but schema governance and auth model still requires hardening.

## 2) Target Architecture (Modern Stack)

### Frontend
- `Next.js` (App Router, TypeScript) as BFF-facing web app.
- UI auth via Supabase Auth (or enterprise IdP via OIDC + Supabase JWT mapping).
- SSR/ISR for dashboard/report pages; client components for interactive clinical workflows.

### Backend microservices
- `Go + Gin`: API gateway/BFF APIs, patient-registration, appointment/order orchestration, authz middleware, integration adapters.
- `Rust`: high-integrity services (audit, hash-chain/anchoring, sensitive record verification, cryptographic utilities).
- API style: external REST/JSON first, selectively add GraphQL for UI aggregation later; use internal gRPC for latency-sensitive service-to-service calls.

### Data platform
- `Supabase` as primary managed PostgreSQL + Auth + Storage + Realtime (when needed).
- Per-service schema ownership (single DB, multiple schemas) with strict migration ownership.
- `Redis` for caching/session/token blacklists/rate-limits.
- `Kafka` for event backbone (record-updated, merge-detected, sync-requested, audit-created).

### Platform & operations
- `Nginx` as ingress/reverse proxy (TLS termination, routing, WAF integration point).
- `Grafana` + Prometheus + Loki/Tempo for metrics/logs/traces with SLO dashboards.

## 3) Service Decomposition Blueprint

### Initial service boundaries (v1)
1. `identity-access-service` (Go)
- user roles/permissions, hospital tenancy mapping, policy enforcement integration.
2. `patient-chart-service` (Go)
- patient demographics, chart metadata, registration references.
3. `order-clinical-service` (Go)
- SOAP/order flows migrated from MVC areas.
4. `lab-transfer-service` (Go)
- ownership of legacy transfer responsibilities.
5. `sync-gateway-service` (Go)
- replace console gateway import/export with API + worker model.
6. `audit-integrity-service` (Rust)
- immutable audit event creation, hash chaining, verification API.
7. `document-proof-service` (Rust)
- record digest creation, Merkle root generation, chain anchoring adapter.

### Anti-corruption layer during migration
- Keep API facade/BFF boundaries to isolate frontend from direct service coupling.
- Migrate by capability, not by project file.
- Maintain compatibility contracts for any remaining external legacy dependencies.

## 4) Phased Execution Plan

### Phase 0: Foundation (2-4 weeks)
- Define target repo structure (monorepo recommended): `apps/web-next`, `services/go/*`, `services/rust/*`, `infra/*`.
- Establish standards for API versioning, tracing IDs, error contracts, schema naming, and CI/CD templates.
- Provision non-prod Supabase, Redis, Kafka, Grafana stack.
- Create data classification policy for HIP/PHI fields and retention.

### Phase 1: Frontend modernization shell (3-5 weeks)
- Bootstrap Next.js app with auth guard, RBAC-aware layout, i18n support.
- Implement BFF route handlers in Next.js only for thin orchestration.
- Start with low-risk screens (login/admin/report read-only) to validate deployment path.
- Route production traffic incrementally through Nginx path-based routing.

### Phase 2: Backend strangler migration (6-12 weeks)
- Extract `lab-transfer-service` and `sync-gateway-service` first (clear boundaries already exist).
- Extract `identity-access-service`; centralize token issuance/validation claims.
- Extract core clinical order flows into `order-clinical-service`.
- Keep legacy MVC endpoints as fallback until parity + replay tests pass.

### Phase 3: Data migration to Supabase (parallel 4-8 weeks)
- Build canonical schema map from current SQL + EF models.
- Create migration pipelines: DDL migration (Flyway/Liquibase/Atlas), backfill jobs (batch + checksum validation), and CDC or dual-write interim strategy for cutover windows.
- Implement RLS policies in Supabase for tenancy + least privilege.

### Phase 4: Event-driven backbone (3-6 weeks)
- Introduce Kafka topics: `patient.events`, `order.events`, `sync.events`, `audit.events`.
- Move cross-service side effects to async consumers.
- Use idempotency keys and outbox pattern per service.

### Phase 5: Observability hardening (2-4 weeks)
- Instrument OpenTelemetry in Next.js, Go, Rust.
- Publish golden signals dashboards in Grafana by service and by hospital tenant.
- Define and monitor SLOs for auth success latency, chart retrieval latency, sync completion SLA, and audit-verification latency.

### Phase 5 Current State
- Implemented:
  - `/metrics` endpoint on Go APIs (`identity`, `patient-chart`, `order-clinical`, `lab-transfer`, `sync-gateway`).
  - Prometheus scrape config and compose deployment.
  - OTEL collector base config and compose deployment.
  - Grafana default datasource switched to Prometheus.
- Pending:
  - service trace exporters and context propagation standardization.
  - production-grade dashboards/alerts per SLO.
  - log and trace backends (Loki/Tempo) full integration.

### Phase 6: Legacy decommission
- Decommission remaining non-Go/Rust backend dependencies after acceptance and legal sign-off.
- Keep rollback runbooks and cutover metrics for 30-day post-cutover period.

## 5) Infrastructure Rollout Order (Requested Components)

1. `Nginx` first (traffic control + strangler pattern).
2. `Supabase` foundation (new schema + auth/RLS baseline).
3. `Redis` (cache/session/rate-limiting).
4. `Kafka` (event contracts + outbox).
5. `Grafana` stack (mandatory before major cutover).

## 6) Security & Compliance Guardrails (HIP/PHI)

- End-to-end TLS, encrypted backups, field-level encryption for highest sensitivity fields.
- Centralized audit log with tamper-evident hash chain.
- Key management through managed KMS/HSM-backed keys.
- RLS + service-level authorization checks (never rely on frontend authorization).
- Data minimization for analytics and observability (no raw PHI in logs/traces).

## 7) Blockchain Features for HIP Data Integrity (Recommendation Set)

### Recommended approach: hybrid anchoring (not on-chain PHI)
- Keep PHI fully off-chain in Supabase/object storage.
- For each clinically signed record event: canonicalize payload, compute SHA-256 digest, append digest to internal hash chain (Rust `audit-integrity-service`), then batch a daily Merkle root and anchor that root hash to a public or consortium chain.
- Store chain tx hash + block height in Supabase audit tables.

### Why this model
- Delivers tamper evidence and independent verifiability.
- Avoids PHI leakage and excessive on-chain cost/latency.
- Supports legal audit: prove “record existed unchanged at/before time T”.

### Candidate blockchain options
1. Permissioned consortium chain (Hyperledger Fabric or Quorum):
- better governance/privacy, more operational overhead.
2. Public L2 anchoring (e.g., Polygon):
- low cost, strong external timestamping, simpler verification tooling.

### Pilot implementation status
- Implemented:
  - `audit-integrity-service` now supports event hashing, chain verification, and Merkle-root generation APIs.
  - `document-proof-service` now supports proof anchoring and anchor verification APIs (simulated chain metadata).
  - both Rust services now support optional Supabase persistence and reconciliation endpoints.
  - Next.js BFF proxy routes and Nginx ingress routes are in place for these Rust services.
  - both Rust services now route repeated persistence failures into dead-letter stores with configurable retry thresholds.
- Pending:
  - replace simulated anchor with real blockchain transaction submission.
  - persist integrity/proof records to Supabase.
  - define legal/audit report format and retention policies.
  - move retry/dead-letter queues to durable backend (Redis/Supabase) instead of process memory.

### Integrity verification features to implement
- `VerifyRecord(record_id)` API returns current digest, historical digest chain, anchored Merkle proof, and chain transaction reference.
- “break-glass” alert if verification fails, with incident workflow.

## 8) Delivery Governance

### Definition of done per migrated capability
- Functional parity tests pass (legacy vs new service).
- Performance non-regression within agreed SLO budget.
- Security review complete (authz, RLS, secrets, audit).
- Runbook + on-call alerts + dashboards present.

### Program controls
- Weekly architecture review and dependency risk tracking.
- Contract testing for all service APIs/events.
- Controlled canary releases per hospital or tenant.

## 9) High-Level Timeline (Indicative)

- Month 1: foundation + Next.js shell + Nginx ingress.
- Month 2-3: first Go services + Supabase schema + Redis + dual-run.
- Month 4: Kafka/event migration + Rust integrity services.
- Month 5: blockchain anchoring pilot + full observability + staged cutover.
- Month 6: legacy retirement window.

## 10) Immediate Next Actions

1. Replace in-memory stores in `identity`, `lab-transfer`, and `sync-gateway` with Supabase + Redis.
2. Move Rust retry/dead-letter state to Redis/Supabase tables and expose dead-letter replay endpoint.
3. Introduce Kafka topic contracts and outbox tables for transfer/sync/audit/retry events.
4. Add service-to-service auth and tenant propagation headers.
5. Implement Grafana dashboards and SLO alerts before broader production traffic cutover.
