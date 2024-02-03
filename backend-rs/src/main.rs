use axum::{routing, Router};
use std::net::SocketAddr;
use tokio::net::TcpListener;

#[tokio::main]
async fn main() {
  let app = Router::new().route("/api/hello-world", routing::get(hello_world));

  let listener = TcpListener::bind(SocketAddr::from(([0, 0, 0, 0], 8601)))
    .await
    .expect("could not bind");

  axum::serve(listener, app).await.expect("could not serve");
}

async fn hello_world() -> &'static str {
  "Hello, world!"
}
