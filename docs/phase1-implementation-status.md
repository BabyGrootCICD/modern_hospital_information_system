# Phase 1 Implementation Status

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
- Real RBAC policy backend wiring (identity-access-service).
- Report summary aggregation from migrated Go services.

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
