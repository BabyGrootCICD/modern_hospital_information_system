# Modern Infra Bootstrap

Run local bootstrap stack:

```bash
docker compose -f infra/docker-compose.modern.yml up -d
```

Included components:
- Nginx
- Next.js web shell
- Redis
- Kafka + Zookeeper
- Grafana

Supabase is planned as managed cloud service and is not containerized in this baseline.

Routing:
- `http://localhost/app/...` -> modern Next.js shell
- `http://localhost/legacy/...` -> legacy stack fallback (defaults to `host.docker.internal:5001`)
- `http://localhost/grafana/` -> Grafana
