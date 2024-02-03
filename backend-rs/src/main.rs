use axum::{routing, Router};
use std::net::SocketAddr;
use tokio::net::TcpListener;
use tower_http::trace::{DefaultMakeSpan, DefaultOnResponse, TraceLayer};
use tracing::{info, Level};

#[tokio::main]
async fn main() {
  tracing_subscriber::fmt()
    .with_target(false)
    .compact()
    .init();

  let app = Router::new()
    .route("/api/hello-world", routing::get(hello_world))
    .layer(
      TraceLayer::new_for_http()
        .make_span_with(DefaultMakeSpan::new().level(Level::INFO))
        .on_response(DefaultOnResponse::new().level(Level::INFO)),
    );

  let listener = TcpListener::bind(SocketAddr::from(([0, 0, 0, 0], 8601)))
    .await
    .expect("could not bind");

  info!(
    "backend available at: {}",
    listener
      .local_addr()
      .expect("could not get listener address")
  );

  axum::serve(listener, app).await.expect("could not serve");
}

async fn hello_world() -> &'static str {
  "Hello, world!"
}
