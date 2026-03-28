# Modern Infra Bootstrap

Run local bootstrap stack:

```bash
docker compose -f infra/docker-compose.modern.yml up -d
```

Included components:
- Nginx
- Redis
- Kafka + Zookeeper
- Grafana

Supabase is planned as managed cloud service and is not containerized in this baseline.
