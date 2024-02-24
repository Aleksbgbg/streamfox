use crate::controllers::errors::{HandlerError, ValidatedJson};
use crate::models::user::{self, CreateUser, User};
use crate::AppState;
use axum::extract::{Request, State};
use axum::http::StatusCode;
use axum::middleware::Next;
use axum::response::Response;
use axum::{Extension, Json};
use axum_extra::extract::cookie::{Cookie, CookieJar};
use chrono::{Duration, Utc};
use entity::id::Id;
use jsonwebtoken::{DecodingKey, EncodingKey, Header, Validation};
use serde::{Deserialize, Serialize};
use std::str::FromStr;
use std::sync::Arc;
use thiserror::Error;
use validator::Validate;

const AUTH_COOKIE_KEY: &str = "Authorization";

#[derive(Debug, Serialize, Deserialize)]
struct Claims {
  exp: usize,
}

fn authenticate(
  jar: CookieJar,
  user: &User,
  lifespan: Duration,
) -> Result<CookieJar, HandlerError> {
  let cookie = Cookie::build((
    AUTH_COOKIE_KEY,
    jsonwebtoken::encode(
      &Header {
        kid: Some(user.id.to_string()),
        ..Default::default()
      },
      &Claims {
        exp: (Utc::now() + lifespan).timestamp().try_into().unwrap(),
      },
      &EncodingKey::from_secret(
        user
          .password
          .as_ref()
          .ok_or(HandlerError::ImpossibleLogin)?
          .as_bytes(),
      ),
    )
    .map_err(HandlerError::EncodeJwt)?,
  ))
  .max_age(time::Duration::seconds(lifespan.num_seconds()))
  .path("/")
  .build();

  Ok(jar.add(cookie))
}

#[derive(Deserialize, Validate)]
#[serde(rename_all = "camelCase")]
pub struct RegisterUser {
  #[validate(length(min = 2, max = 32))]
  username: String,
  #[validate(email)]
  email_address: String,
  #[validate(length(min = 6, max = 72))]
  password: String,
  #[validate(must_match(other = "password", message = "Password"))]
  repeat_password: String,
}

pub async fn register(
  State(state): State<AppState>,
  cookies: CookieJar,
  ValidatedJson(details): ValidatedJson<RegisterUser>,
) -> Result<(StatusCode, CookieJar), HandlerError> {
  if user::name_exists(&state.connection, &details.username).await? {
    return Err(HandlerError::UsernameTaken);
  }
  if user::email_exists(&state.connection, &details.email_address).await? {
    return Err(HandlerError::EmailTaken);
  }

  let user = user::create(
    &state.connection,
    &state.snowflakes,
    CreateUser {
      username: details.username,
      email_address: details.email_address,
      password: &details.password,
    },
  )
  .await?;

  let cookies = authenticate(cookies, &user, state.config.app.token_lifespan)?;

  Ok((StatusCode::CREATED, cookies))
}

#[derive(Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct LoginUser {
  username: String,
  password: String,
}

pub async fn login(
  State(state): State<AppState>,
  cookies: CookieJar,
  Json(details): Json<LoginUser>,
) -> Result<(StatusCode, CookieJar), HandlerError> {
  if let Some(user) =
    user::login_with_credentials(&state.connection, &details.username, &details.password).await?
  {
    let cookies = authenticate(cookies, &user, state.config.app.token_lifespan)?;

    Ok((StatusCode::CREATED, cookies))
  } else {
    Err(HandlerError::InvalidCredentials)
  }
}

#[derive(Error, Debug)]
pub enum AuthError {
  #[error("authorization cookie not present")]
  NoAuthCookie,
  #[error("could not decode header")]
  DecodeHeader(jsonwebtoken::errors::Error),
  #[error("user ID was not present")]
  NoKid,
  #[error("could not decode user ID")]
  DecodeId(bs58::decode::Error),
  #[error("user ID was not found")]
  UserNotFound,
  #[error("could not decode authentication token")]
  DecodeJwt(jsonwebtoken::errors::Error),
}

pub async fn extract(
  State(state): State<AppState>,
  cookies: CookieJar,
  mut request: Request,
  next: Next,
) -> Result<Response, HandlerError> {
  let token = cookies
    .get(AUTH_COOKIE_KEY)
    .ok_or(AuthError::NoAuthCookie)?
    .value();
  let id = jsonwebtoken::decode_header(token)
    .map_err(AuthError::DecodeHeader)?
    .kid
    .ok_or(AuthError::NoKid)?;
  let user = user::find(
    &state.connection,
    Id::from_str(&id).map_err(AuthError::DecodeId)?,
  )
  .await?
  .ok_or(AuthError::UserNotFound)?;
  let _claims = jsonwebtoken::decode::<Claims>(
    token,
    &DecodingKey::from_secret(
      user
        .password
        .as_ref()
        .ok_or(HandlerError::ImpossibleLogin)?
        .as_bytes(),
    ),
    &Validation::default(),
  )
  .map_err(AuthError::DecodeJwt)?
  .claims;

  request.extensions_mut().insert(Arc::new(user));

  Ok(next.run(request).await)
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct UserResponse {
  id: Id,
  username: String,
}

impl From<User> for UserResponse {
  fn from(value: User) -> Self {
    UserResponse {
      id: value.id,
      username: value.username,
    }
  }
}

impl From<&User> for UserResponse {
  fn from(value: &User) -> Self {
    UserResponse {
      id: value.id,
      username: value.username.clone(),
    }
  }
}

pub async fn get_user(Extension(user): Extension<Arc<User>>) -> Json<UserResponse> {
  Json(user.as_ref().into())
}
