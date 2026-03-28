# Supabase Data Layer Bootstrap

This folder contains the Phase 3 migration baseline for Supabase PostgreSQL.

## Migration files

- `migrations/0001_core_schemas.sql`: service-owned schemas + core tables.
- `migrations/0002_rls_baseline.sql`: initial RLS policies for service-role access.
- `seeds/0001_patient_charts.sql`: seed records for chart API smoke tests.

## Apply order

1. `0001_core_schemas.sql`
2. `0002_rls_baseline.sql`
3. `seeds/0001_patient_charts.sql` (optional for local test data)

## Notes

- This phase intentionally keeps policies simple (`service_role`) to unblock migration.
- Tenant-aware and role-aware end-user RLS rules are planned in the next hardening pass.
