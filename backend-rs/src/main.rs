mod config;

use crate::config::ConfigError;
use axum::{routing, Router};
use std::io;
use std::net::SocketAddr;
use thiserror::Error;
use tokio::net::TcpListener;
use tower_http::trace::{DefaultMakeSpan, DefaultOnResponse, TraceLayer};
use tracing::{info, Level};

#[derive(Error, Debug)]
enum AppError {
  #[error("could not load config")]
  LoadConfig(#[from] ConfigError),
  #[error("could not bind to network interface")]
  BindTcpListener(io::Error),
  #[error("could not get TCP listener address")]
  GetListenerAddress(io::Error),
  #[error("could not start Axum server")]
  ServeApp(io::Error),
}

#[tokio::main]
async fn main() -> Result<(), AppError> {
  let config = config::load()?;

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

  let listener = TcpListener::bind(SocketAddr::from((config.app.host, config.app.port)))
    .await
    .map_err(AppError::BindTcpListener)?;

  info!(
    "backend available at: {}",
    listener
      .local_addr()
      .map_err(AppError::GetListenerAddress)?
  );

  axum::serve(listener, app)
    .await
    .map_err(AppError::ServeApp)?;

  Ok(())
}

async fn hello_world() -> &'static str {
  "Hello, world!"
}
