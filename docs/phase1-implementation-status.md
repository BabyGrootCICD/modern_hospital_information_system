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
