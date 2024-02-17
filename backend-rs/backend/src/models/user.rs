use crate::models::base::exists;
use crate::models::id::Id;
use crate::secure::{random_bytes, Bytes};
use crate::{MainFs, Snowflakes};
use base64::engine::general_purpose;
use base64::Engine;
use bcrypt::BcryptError;
use chrono::Local;
use ring::error::Unspecified;
use ring::rand::SystemRandom;
use sea_orm::entity::prelude::*;
use sea_orm::ActiveValue;
use thiserror::Error;
use tokio::io::AsyncWriteExt;
pub use Model as User;

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "user")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub id: Id,
  pub created_at: DateTimeWithTimeZone,
  pub updated_at: DateTimeWithTimeZone,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub username: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub canonical_username: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub email_address: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub canonical_email_address: Option<String>,
  #[sea_orm(column_type = "Text", nullable)]
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

#[derive(Error, Debug)]
pub enum CreateDefaultUsersError {
  #[error(transparent)]
  Database(#[from] DbErr),
  #[error(transparent)]
  Io(#[from] std::io::Error),
  #[error(transparent)]
  Create(#[from] CreateError),
  #[error(transparent)]
  GeneratePassword(#[from] Unspecified),
}

pub async fn create_default_users(
  connection: &DatabaseConnection,
  fs: &MainFs,
) -> Result<(), CreateDefaultUsersError> {
  const STREAMFOX_ID: Id = Id::from(1);
  const ANONYMOUS_ID: Id = Id::from(2);
  const DELETED_ID: Id = Id::from(3);

  if !id_exists(connection, STREAMFOX_ID).await? {
    let mut file = fs.streamfox_default_password().create().await?;
    let password = general_purpose::STANDARD.encode(random_bytes(&SystemRandom::new(), Bytes(32))?);
    file.write_all(password.as_bytes()).await?;

    create_base(
      connection,
      STREAMFOX_ID,
      Some("Streamfox"),
      None,
      Some(&password),
    )
    .await?;
  }

  if !id_exists(connection, ANONYMOUS_ID).await? {
    create_base(connection, ANONYMOUS_ID, Some("Anonymous"), None, None).await?;
  }

  if !id_exists(connection, DELETED_ID).await? {
    create_base(connection, DELETED_ID, Some("Deleted"), None, None).await?;
  }

  Ok(())
}

async fn id_exists(connection: &DatabaseConnection, id: Id) -> Result<bool, DbErr> {
  exists::<Entity>(connection, Column::Id, id).await
}

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
  create_base(
    connection,
    Id::from(snowflakes.user_snowflake.lock().await.get_id()),
    Some(user.username),
    Some(user.email_address),
    Some(user.password),
  )
  .await
}

async fn create_base(
  connection: &DatabaseConnection,
  id: Id,
  username: Option<&str>,
  email_address: Option<&str>,
  password: Option<&str>,
) -> Result<User, CreateError> {
  let time = Local::now().fixed_offset();
  let hashed_pasword = {
    if let Some(password) = password {
      Some(bcrypt::hash(password, 10)?)
    } else {
      None
    }
  };

  Ok(
    ActiveModel {
      id: ActiveValue::set(id),
      created_at: ActiveValue::set(time),
      updated_at: ActiveValue::set(time),
      username: ActiveValue::set(username.map(str::to_string)),
      canonical_username: ActiveValue::set(username.map(str::to_lowercase)),
      email_address: ActiveValue::set(email_address.map(str::to_string)),
      canonical_email_address: ActiveValue::set(email_address.map(str::to_lowercase)),
      password: ActiveValue::set(hashed_pasword),
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
