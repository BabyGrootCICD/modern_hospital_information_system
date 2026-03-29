# Phase 1 Implementation Status

Status: Completed, with follow-up items moved into Phase 2/3.

Backend policy update: repository now excludes .NET backend projects; Go/Rust is the only backend path.

Observability note: frontend routes are now served behind infra that includes Prometheus/OTEL plumbing for backend services.
Integrity note: frontend BFF now also proxies integrity/proof APIs from Rust services.
Durability note: integrity/proof APIs now expose reconciliation endpoints for persistence checks.
Reliability note: integrity/proof APIs now include retry queue status and manual retry trigger endpoints.
Resilience note: integrity/proof retries now include dead-letter routing with configurable failure thresholds.
Durability note: integrity/proof retry/dead-letter state now persists to Redis and supports dead-letter replay endpoints.

## Completed in this batch

- Next.js locale route shell (`/en`, `/zh`) with middleware redirect behavior.
- Session login/logout endpoints and cookie-based protected route gate.
- RBAC-oriented pages:
  - dashboard
  - admin (admin role expected)
  - reports (read-only via BFF route)
- BFF endpoints:
  - `/api/bff/health`
  - `/api/bff/reports/summary`
- Nginx path-based routing for strangler rollout:
  - `/app/*` -> Next.js
  - `/legacy/*` -> legacy app endpoint

## Deferred to next batch

- Supabase Auth integration replacing temporary cookie session.
- End-to-end RBAC enforcement via identity service token validation.
- Report summary aggregation from migrated Go services and event stream.

## Follow-up completed in Phase 2 kickoff

- `identity-access-service` now provides:
  - `/v1/auth/login`
  - `/v1/auth/validate`
  - `/v1/rbac/policies/:role`
- `lab-transfer-service` now provides:
  - `/v1/transfers`
  - `/v1/transfers/:id`
  - `/v1/transfers/:id/complete`
- `sync-gateway-service` now provides:
  - `/v1/sync/jobs`
  - `/v1/sync/jobs/:id/start`
  - `/v1/sync/jobs/:id/complete`
  - `/v1/sync/jobs/:id`
- Nginx routes added:
  - `/api/identity/*`
  - `/api/lab/*`
  - `/api/sync/*`

## Potential TODOs

- Add end-to-end locale + auth redirect tests in Playwright.
- Add CSP and security headers validation for all frontend routes.
