FROM rust:1.81-bookworm AS builder
WORKDIR /src

COPY Cargo.toml Cargo.lock ./
COPY src ./src
RUN cargo build --release

FROM debian:bookworm-slim
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl \
    && rm -rf /var/lib/apt/lists/* \
    && useradd -u 10001 -m app
WORKDIR /app
COPY --from=builder /src/target/release/document-proof-service /app/document-proof-service
USER app
EXPOSE 8092
ENTRYPOINT ["/app/document-proof-service"]
