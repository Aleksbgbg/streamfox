use sea_orm::{ColumnTrait, DatabaseConnection, DbErr, EntityTrait, QueryFilter, Value};

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
