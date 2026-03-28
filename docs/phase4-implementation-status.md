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

## Pending for full Phase 4 completion

- Outbox table and transactional outbox writer per service.
- Idempotent event consumers with replay and dead-letter handling.
- Schema/version contracts for event payloads.
- Audit event stream integration with Rust integrity services.
