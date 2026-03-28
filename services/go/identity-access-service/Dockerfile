FROM golang:1.23-alpine AS builder
WORKDIR /src

COPY go.mod go.sum ./
RUN go mod download

COPY cmd ./cmd
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -trimpath -ldflags="-s -w" -o /out/api ./cmd/api

FROM alpine:3.21
WORKDIR /app
COPY --from=builder /out/api /app/api
USER 65532:65532
EXPOSE 8081
ENTRYPOINT ["/app/api"]
