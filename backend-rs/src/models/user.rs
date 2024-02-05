use crate::app_state::AppState;
use chrono::Local;
use sea_orm::entity::prelude::*;
use sea_orm::ActiveValue;

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "user")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub id: i64,
  pub created_at: DateTimeWithTimeZone,
  pub updated_at: DateTimeWithTimeZone,
  #[sea_orm(unique)]
  pub username: Option<String>,
  #[sea_orm(unique)]
  pub canonical_username: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub email_address: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub canonical_email_address: Option<String>,
  pub password: Option<String>,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {}

impl ActiveModelBehavior for ActiveModel {}
