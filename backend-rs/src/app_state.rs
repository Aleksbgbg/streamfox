use sea_orm::DatabaseConnection;
use snowflake::SnowflakeIdBucket;

#[derive(Clone)]
pub struct AppState {
  pub connection: DatabaseConnection,
  pub user_snowflake: SnowflakeIdBucket,
}
