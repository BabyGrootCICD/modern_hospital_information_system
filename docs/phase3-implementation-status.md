# Phase 3 Implementation Status

Status: Kickoff implemented (schema baseline + service integration); migration execution pending.

## Implemented

- Supabase migration baseline artifacts:
  - `infra/supabase/migrations/0001_core_schemas.sql`
  - `infra/supabase/migrations/0002_rls_baseline.sql`
  - `infra/supabase/seeds/0001_patient_charts.sql`
- Supabase-oriented `patient-chart-service` read path:
  - `GET /v1/charts/:patientID` reads from Supabase REST when env vars are present.
  - Fallback mock response when env vars are absent.
- Nginx/compose routes for chart API:
  - `/api/charts/*`
- Next.js BFF bridge:
  - `/api/bff/charts/:patientID`

## Pending for full Phase 3 completion

- Legacy-to-Supabase backfill and checksum validation jobs.
- CDC/dual-write cutover mechanism.
- Tenant-aware RLS policies beyond `service_role` baseline.
- Full schema mapping report for all legacy tables.
- Redis caching integration for high-volume read paths.
- Supabase operational dashboards in Grafana tied to migration cutover KPIs.
- Integrity and proof tables for Rust services (chain hash / anchor tx metadata) migration scripts.
- Reconciliation endpoints can now be used to validate Supabase persistence readiness.
