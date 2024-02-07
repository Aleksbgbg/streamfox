use crate::app_state::AppState;
use crate::models::base::exists;
use bcrypt::BcryptError;
use chrono::Local;
use sea_orm::entity::prelude::*;
use sea_orm::ActiveValue;
use thiserror::Error;

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

pub async fn name_exists(state: &AppState, name: &str) -> Result<bool, DbErr> {
  exists::<Entity>(
    &state.connection,
    Column::CanonicalUsername,
    name.to_lowercase(),
  )
  .await
}

pub async fn email_exists(state: &AppState, email: &str) -> Result<bool, DbErr> {
  exists::<Entity>(
    &state.connection,
    Column::CanonicalEmailAddress,
    email.to_lowercase(),
  )
  .await
}

pub struct CreateUser<'a> {
  pub username: &'a str,
  pub email_address: &'a str,
  pub password: &'a str,
}

#[derive(Error, Debug)]
pub enum CreateError {
  #[error(transparent)]
  Database(#[from] DbErr),
  #[error(transparent)]
  Hash(#[from] BcryptError),
}

pub async fn create(state: &mut AppState, user: CreateUser<'_>) -> Result<(), CreateError> {
  let id = state.user_snowflake.get_id();
  let time = Local::now().fixed_offset();
  let hashed_pasword = bcrypt::hash(user.password, bcrypt::DEFAULT_COST)?;

  Entity::insert(ActiveModel {
    id: ActiveValue::set(id),
    created_at: ActiveValue::set(time),
    updated_at: ActiveValue::set(time),
    username: ActiveValue::set(Some(user.username.to_owned())),
    canonical_username: ActiveValue::set(Some(user.username.to_lowercase())),
    email_address: ActiveValue::set(Some(user.email_address.to_owned())),
    canonical_email_address: ActiveValue::set(Some(user.email_address.to_lowercase())),
    password: ActiveValue::set(Some(hashed_pasword)),
  })
  .exec(&state.connection)
  .await?;

  Ok(())
}
