# SHIS Modern Backend Workspace

This repository now uses:
- Frontend: Next.js (`apps/web-next`)
- Backend APIs: Go-Gin (`services/go/*`) and Rust (`services/rust/*`)
- Data layer: Supabase migration assets (`infra/supabase`)
- Event backbone: Kafka (`infra/kafka`, compose wiring)

## Quick start (local infra)

```bash
docker compose -f infra/docker-compose.modern.yml up -d
```

## Core API routes via Nginx

- `/app/*` -> Next.js frontend shell
- `/api/identity/*` -> identity-access-service
- `/api/charts/*` -> patient-chart-service
- `/api/lab/*` -> lab-transfer-service
- `/api/sync/*` -> sync-gateway-service
- `/api/integrity/*` -> audit-integrity-service (Rust)
- `/api/proofs/*` -> document-proof-service (Rust)
- `/grafana/*` -> Grafana
- `/prometheus/*` -> Prometheus

## Notes

- .NET backend projects were intentionally removed.
- Legacy SQL snapshots remain in repository root as migration references.
- Detailed phase progress is maintained in `docs/phase*-implementation-status.md` (Phase 1-6).
