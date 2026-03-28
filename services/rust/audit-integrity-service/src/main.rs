use axum::{routing::get, Json, Router};
use serde::Serialize;

#[derive(Serialize)]
struct Health<'a> {
    service: &'a str,
    status: &'a str,
}

#[tokio::main]
async fn main() {
    let app = Router::new().route(
        "/healthz",
        get(|| async { Json(Health { service: "audit-integrity-service", status: "ok" }) }),
    );

    let listener = tokio::net::TcpListener::bind("0.0.0.0:8091")
        .await
        .expect("bind failed");
    axum::serve(listener, app).await.expect("server failed");
}
