use crate::app_state::AppState;
use crate::controllers::errors::HandlerError;
use crate::controllers::validate::ValidatedJson;
use crate::models::user::{self, CreateUser};
use axum::extract::State;
use axum::http::StatusCode;
use serde::Deserialize;
use validator::Validate;

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
  ValidatedJson(details): ValidatedJson<RegisterUser>,
) -> Result<StatusCode, HandlerError> {
  if user::name_exists(&state, &details.username).await? {
    return Err(HandlerError::UsernameTaken);
  }
  if user::email_exists(&state, &details.email_address).await? {
    return Err(HandlerError::EmailTaken);
  }

  user::create(
    &mut state,
    CreateUser {
      username: &details.username,
      email_address: &details.email_address,
      password: &details.password,
    },
  )
  .await?;

  Ok(StatusCode::CREATED)
}
