mod config;
mod models;

use crate::config::ConfigError;
use crate::models::migrations::migrator::Migrator;
use axum::{routing, Router};
use sea_orm::{Database, DbErr};
use sea_orm_migration::MigratorTrait;
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
  #[error("could not connect to database")]
  ConnectToDatabase(DbErr),
  #[error("could not run database migrations")]
  MigrateDatabase(DbErr),
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

  let connection = Database::connect(&config.database.connection_string())
    .await
    .map_err(AppError::ConnectToDatabase)?;
  Migrator::up(&connection, None)
    .await
    .map_err(AppError::MigrateDatabase)?;

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
