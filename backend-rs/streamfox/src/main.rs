mod config;
mod controllers;
mod models;
mod secure;
mod snowflake;

use crate::config::{Config, ConfigError};
use crate::controllers::{user, video};
use crate::models::user::CreateDefaultUsersError;
use crate::snowflake::SnowflakeGenerator;
use axum::{middleware, routing, Router};
use cascade::cascade;
use fs::filesystem;
use migration::migrator::Migrator;
use sea_orm::{ConnectOptions, Database, DatabaseConnection, DbErr};
use sea_orm_migration::migrator::MigratorTrait;
use std::io;
use std::net::SocketAddr;
use std::sync::Arc;
use thiserror::Error;
use tokio::net::TcpListener;
use tower_http::trace::{DefaultMakeSpan, DefaultOnResponse, TraceLayer};
use tracing::{info, Level};

filesystem!(
  MainFs,
  r"
<ConfigRoot>
  streamfox_default_password #streamfox_default_password
"
);

#[derive(Error, Debug)]
enum AppError {
  #[error("could not load config")]
  LoadConfig(#[from] ConfigError),
  #[error("could not connect to database")]
  ConnectToDatabase(DbErr),
  #[error("could not run database migrations")]
  MigrateDatabase(DbErr),
  #[error("could not create default users")]
  CreateDefaultUsers(#[from] CreateDefaultUsersError),
  #[error("could not bind to network interface")]
  BindTcpListener(io::Error),
  #[error("could not get TCP listener address")]
  GetListenerAddress(io::Error),
  #[error("could not start Axum server")]
  ServeApp(io::Error),
}

#[derive(Clone)]
struct AppState {
  config: Arc<Config>,
  connection: DatabaseConnection,
  snowflakes: Arc<Snowflakes>,
}

struct Snowflakes {
  user_snowflake: SnowflakeGenerator,
  video_snowflake: SnowflakeGenerator,
}

#[tokio::main]
async fn main() -> Result<(), AppError> {
  let config = Arc::new(config::load()?);
  let fs = cascade! {
    MainFs::new();
    ..set(MainFsVar::ConfigRoot, config.app.config_root.clone());
  };

  tracing_subscriber::fmt()
    .with_target(false)
    .compact()
    .with_max_level(tracing::Level::DEBUG)
    .init();

  let connection = Database::connect(cascade! {
    ConnectOptions::new(config.database.connection_string());
    ..sqlx_logging(false);
  })
  .await
  .map_err(AppError::ConnectToDatabase)?;
  Migrator::up(&connection, None)
    .await
    .map_err(AppError::MigrateDatabase)?;

  models::user::create_default_users(&connection, &fs).await?;

  let listener = TcpListener::bind(SocketAddr::from((config.app.host, config.app.port)))
    .await
    .map_err(AppError::BindTcpListener)?;

  let state = AppState {
    config,
    connection,
    snowflakes: Arc::new(Snowflakes {
      user_snowflake: SnowflakeGenerator::new(0),
      video_snowflake: SnowflakeGenerator::new(1),
    }),
  };

  let unauthenticated = Router::new()
    .route("/auth/register", routing::post(user::register))
    .route("/auth/login", routing::post(user::login))
    .route("/videos", routing::get(video::get_videos));
  let authenticated = Router::new()
    .route("/user", routing::get(user::get_user))
    .route("/videos", routing::post(video::create_video))
    .layer(middleware::from_fn_with_state(state.clone(), user::extract));
  let app = Router::new()
    .merge(unauthenticated)
    .merge(authenticated)
    .layer(
      TraceLayer::new_for_http()
        .make_span_with(DefaultMakeSpan::new().level(Level::INFO))
        .on_response(DefaultOnResponse::new().level(Level::INFO)),
    )
    .with_state(state);
  let api = Router::new().nest("/api", app);

  info!(
    "backend available at: {}",
    listener
      .local_addr()
      .map_err(AppError::GetListenerAddress)?
  );

  axum::serve(listener, api)
    .await
    .map_err(AppError::ServeApp)?;

  Ok(())
}
