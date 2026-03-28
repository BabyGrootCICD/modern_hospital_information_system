# SHIS Modernization Migration Plan

## 1) Current-State Scan Summary

### Existing solution layout
- `KMU.HisOrder.MVC` (`net6.0`): monolithic ASP.NET MVC + Razor + SignalR + EF Core/PostgreSQL.
- `KMU.MOHD.WebAPI` (`net6.0`): JWT-based API for MOHD-side records and upload tasks.
- `KMU.Lab.WebAPI` (`net6.0`): transfer-data API with EF migrations and Swagger.
- `KMU.HisOrder.Console.Gateway` (`net6.0`): console/offline synchronization gateway with import/export flow.
- SQL snapshots/dumps exist at repository root, indicating schema-first operational history.

### Key migration implications
- Domain logic is spread across MVC areas/controllers/services; needs explicit bounded-context split before service extraction.
- Offline synchronization in console gateway is business-critical and should become first-class microservice workflows.
- Existing PostgreSQL usage lowers migration risk to Supabase (also PostgreSQL), but schema governance and auth model must be redesigned.

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
- ownership of current `KMU.Lab.WebAPI` transfer responsibilities.
5. `sync-gateway-service` (Go)
- replace console gateway import/export with API + worker model.
6. `audit-integrity-service` (Rust)
- immutable audit event creation, hash chaining, verification API.
7. `document-proof-service` (Rust)
- record digest creation, Merkle root generation, chain anchoring adapter.

### Anti-corruption layer during migration
- Keep legacy .NET apps running behind Nginx while new services are carved out.
- Add API facade layer to prevent frontend coupling to legacy controllers.
- Migrate by capability, not by project file.

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

### Phase 6: Legacy decommission
- Freeze legacy .NET write paths.
- Run read-only mode for rollback window.
- Decommission MVC/WebAPI/console components after acceptance and legal sign-off.

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

1. Approve service boundaries and migration order in this document.
2. Decide blockchain mode:
- consortium only, public L2 anchoring, or dual anchor (highest assurance).
3. Create implementation epics from phases (frontend, services, data, infra, integrity).
4. Start Phase 0 with baseline repository and CI/CD scaffolding.
