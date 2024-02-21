use crate::snowflake::SnowflakeGenerator;
use entity::id::Id;
use sea_orm::{ColumnTrait, DatabaseConnection, DbErr, EntityTrait, QueryFilter, Value};

pub async fn new_id(snowflake: &SnowflakeGenerator) -> Id {
  Id::from(snowflake.generate_id().await)
}

pub async fn exists<E: EntityTrait>(
  connection: &DatabaseConnection,
  column: impl ColumnTrait,
  value: impl Into<Value>,
) -> Result<bool, DbErr> {
  Ok(
    E::find()
      .filter(column.eq(value))
      .one(connection)
      .await?
      .is_some(),
  )
}
