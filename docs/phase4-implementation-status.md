# Phase 4 Implementation Status

Status: Kickoff implemented; durable event pipeline hardening pending.

## Implemented

- Kafka publish hooks added to:
  - `identity-access-service` (`identity.events`)
  - `lab-transfer-service` (`transfer.events`)
  - `sync-gateway-service` (`sync.events`)
- Local Kafka topic bootstrap script:
  - `infra/kafka/bootstrap-topics.sh`
- Runtime wiring:
  - `KAFKA_BROKERS` env injected via compose for Go services.
- Phase 5 integration linkage:
  - Prometheus now scrapes all Go service metrics while event endpoints are active.

## Pending for full Phase 4 completion

- Outbox table and transactional outbox writer per service.
- Idempotent event consumers with replay and dead-letter handling.
- Schema/version contracts for event payloads.
- Audit event stream integration with Rust integrity services.
- Event-driven trigger from transfer/sync completion to integrity event creation.
- Durable outbox integration with integrity/proof persistence flows.
- Retry/dead-letter events should be emitted to Kafka in the next pass for failure observability.
