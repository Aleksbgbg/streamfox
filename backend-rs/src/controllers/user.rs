use crate::app_state::AppState;
use crate::controllers::errors::HandlerError;
use crate::controllers::validate::ValidatedJson;
use crate::models::user::{self, CreateUser, User};
use axum::extract::State;
use axum::http::StatusCode;
use axum_extra::extract::cookie::{Cookie, CookieJar};
use chrono::{Duration, Utc};
use jsonwebtoken::{EncodingKey, Header};
use serde::{Deserialize, Serialize};
use thiserror::Error;
use validator::Validate;

const AUTH_COOKIE_KEY: &str = "Authorization";
const SECRET: &str = "123456";

#[derive(Debug, Serialize, Deserialize)]
struct Claims {
  uid: String,
  exp: usize,
}

#[derive(Error, Debug)]
pub enum AuthError {
  #[error("could not encode JWT")]
  EncodeJwt(#[from] jsonwebtoken::errors::Error),
}

fn authenticate(jar: CookieJar, user: &User, lifespan: Duration) -> Result<CookieJar, AuthError> {
  let cookie = Cookie::build((
    AUTH_COOKIE_KEY,
    jsonwebtoken::encode(
      &Header::default(),
      &Claims {
        uid: user.id.to_string(),
        exp: (Utc::now() + lifespan).timestamp().try_into().unwrap(),
      },
      &EncodingKey::from_secret(SECRET.as_ref()),
    )?,
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
  State(mut state): State<AppState>,
  cookies: CookieJar,
  ValidatedJson(details): ValidatedJson<RegisterUser>,
) -> Result<(StatusCode, CookieJar), HandlerError> {
  if user::name_exists(&state, &details.username).await? {
    return Err(HandlerError::UsernameTaken);
  }
  if user::email_exists(&state, &details.email_address).await? {
    return Err(HandlerError::EmailTaken);
  }

  let user = user::create(
    &mut state,
    CreateUser {
      username: &details.username,
      email_address: &details.email_address,
      password: &details.password,
    },
  )
  .await?;

  let cookies = authenticate(cookies, &user, state.config.app.token_lifespan)?;

  Ok((StatusCode::CREATED, cookies))
}
