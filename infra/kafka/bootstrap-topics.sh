#!/usr/bin/env sh
set -eu

BROKER="${1:-localhost:9092}"

create_topic() {
  TOPIC="$1"
  docker exec -i "$(docker ps --filter name=kafka --format '{{.ID}}' | head -n 1)" \
    kafka-topics --bootstrap-server "$BROKER" --create --if-not-exists \
    --topic "$TOPIC" --replication-factor 1 --partitions 3
}

create_topic "identity.events"
create_topic "transfer.events"
create_topic "sync.events"
create_topic "audit.events"

echo "Kafka topics ensured."
