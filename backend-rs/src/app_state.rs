use crate::config::Config;
use sea_orm::DatabaseConnection;
use snowflake::SnowflakeIdBucket;
use std::sync::Arc;

#[derive(Clone)]
pub struct AppState {
  pub config: Arc<Config>,
  pub connection: DatabaseConnection,
  pub user_snowflake: SnowflakeIdBucket,
}
