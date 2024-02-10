use crate::models::base::exists;
use crate::models::id::Id;
use crate::Snowflakes;
use bcrypt::BcryptError;
use chrono::Local;
use sea_orm::entity::prelude::*;
use sea_orm::ActiveValue;
use thiserror::Error;
pub use Model as User;

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "user")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub id: Id,
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

impl Model {
  pub fn visible_username(&self) -> &str {
    self.username.as_ref().map_or("Anonymous", String::as_str)
  }
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {}

impl ActiveModelBehavior for ActiveModel {}

pub async fn name_exists(connection: &DatabaseConnection, name: &str) -> Result<bool, DbErr> {
  exists::<Entity>(connection, Column::CanonicalUsername, name.to_lowercase()).await
}

pub async fn email_exists(connection: &DatabaseConnection, email: &str) -> Result<bool, DbErr> {
  exists::<Entity>(
    connection,
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

pub async fn create(
  connection: &DatabaseConnection,
  snowflakes: &Snowflakes,
  user: CreateUser<'_>,
) -> Result<User, CreateError> {
  let id = snowflakes.user_snowflake.lock().await.get_id();
  let time = Local::now().fixed_offset();
  let hashed_pasword = bcrypt::hash(user.password, 10)?;

  Ok(
    ActiveModel {
      id: ActiveValue::set(Id::from(id)),
      created_at: ActiveValue::set(time),
      updated_at: ActiveValue::set(time),
      username: ActiveValue::set(Some(user.username.to_owned())),
      canonical_username: ActiveValue::set(Some(user.username.to_lowercase())),
      email_address: ActiveValue::set(Some(user.email_address.to_owned())),
      canonical_email_address: ActiveValue::set(Some(user.email_address.to_lowercase())),
      password: ActiveValue::set(Some(hashed_pasword)),
    }
    .insert(connection)
    .await?,
  )
}

#[derive(Error, Debug)]
pub enum ValidateCredentialsError {
  #[error(transparent)]
  Database(#[from] DbErr),
  #[error(transparent)]
  Hash(#[from] BcryptError),
}

pub async fn login_with_credentials(
  connection: &DatabaseConnection,
  name: &str,
  password: &str,
) -> Result<Option<User>, ValidateCredentialsError> {
  let user = Entity::find()
    .filter(Column::Username.eq(name))
    .one(connection)
    .await?;

  match user {
    None => Ok(None),
    Some(user) => match &user.password {
      None => Ok(None),
      Some(hash) => {
        if bcrypt::verify(password, hash)? {
          Ok(Some(user))
        } else {
          Ok(None)
        }
      }
    },
  }
}

pub async fn find(connection: &DatabaseConnection, id: Id) -> Result<Option<User>, DbErr> {
  Entity::find_by_id(id).one(connection).await
}
