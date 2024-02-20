use crate::models::base::exists;
use crate::secure::{random_bytes, Bytes};
use crate::{MainFs, Snowflakes};
use base64::engine::general_purpose;
use base64::Engine;
use bcrypt::BcryptError;
use chrono::Local;
use entity::id::Id;
use entity::user;
use ring::error::Unspecified;
use ring::rand::SystemRandom;
use sea_orm::prelude::*;
use sea_orm::ActiveValue;
use thiserror::Error;
use tokio::io::AsyncWriteExt;

pub struct User {
  pub id: Id,
  pub username: String,
  pub password: Option<String>,
}

impl From<user::Model> for User {
  fn from(value: user::Model) -> Self {
    User {
      id: value.id,
      username: value.username.unwrap_or("Anonymous".into()),
      password: value.password,
    }
  }
}

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
  exists::<user::Entity>(connection, user::Column::Id, id).await
}

pub async fn name_exists(connection: &DatabaseConnection, name: &str) -> Result<bool, DbErr> {
  exists::<user::Entity>(
    connection,
    user::Column::CanonicalUsername,
    name.to_lowercase(),
  )
  .await
}

pub async fn email_exists(connection: &DatabaseConnection, email: &str) -> Result<bool, DbErr> {
  exists::<user::Entity>(
    connection,
    user::Column::CanonicalEmailAddress,
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
  Ok(
    create_base(
      connection,
      Id::from(snowflakes.user_snowflake.generate_id().await),
      Some(user.username),
      Some(user.email_address),
      Some(user.password),
    )
    .await?
    .into(),
  )
}

async fn create_base(
  connection: &DatabaseConnection,
  id: Id,
  username: Option<&str>,
  email_address: Option<&str>,
  password: Option<&str>,
) -> Result<user::Model, CreateError> {
  let time = Local::now().fixed_offset();
  let hashed_pasword = {
    if let Some(password) = password {
      Some(bcrypt::hash(password, 10)?)
    } else {
      None
    }
  };

  Ok(
    user::ActiveModel {
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
  let user = user::Entity::find()
    .filter(user::Column::Username.eq(name))
    .one(connection)
    .await?;

  match user {
    None => Ok(None),
    Some(user) => match &user.password {
      None => Ok(None),
      Some(hash) => {
        if bcrypt::verify(password, hash)? {
          Ok(Some(user.into()))
        } else {
          Ok(None)
        }
      }
    },
  }
}

pub async fn find(connection: &DatabaseConnection, id: Id) -> Result<Option<User>, DbErr> {
  Ok(
    user::Entity::find_by_id(id)
      .one(connection)
      .await?
      .map(Into::into),
  )
}
