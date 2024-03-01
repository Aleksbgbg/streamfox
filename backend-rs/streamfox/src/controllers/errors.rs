use crate::codec::{GenerateThumbnailError, ProbeError};
use crate::controllers::user::AuthError;
use crate::models::user;
use axum::extract::rejection::{JsonRejection, PathRejection};
use axum::extract::{FromRequest, Request};
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use axum::{async_trait, Json};
use convert_case::{Case, Casing};
use lazy_static::lazy_static;
use sea_orm::DbErr;
use serde::de::DeserializeOwned;
use serde::Serialize;
use serde_json::json;
use std::collections::HashMap;
use std::num::TryFromIntError;
use thiserror::Error;
use tracing::error;
use validator::{Validate, ValidationErrors, ValidationErrorsKind};

lazy_static! {
  static ref FAILED_VALIDATION: Errors =
    Errors::generic("Some inputs failed validation.".to_string());
}

#[derive(Serialize, Default, Clone)]
struct Errors {
  generic: Vec<String>,
  specific: HashMap<String, Vec<String>>,
}

impl Errors {
  fn generic(value: String) -> Self {
    let mut errors = Self::default();
    errors.add_generic(value);
    errors
  }

  fn add_generic(&mut self, value: String) {
    self.generic.push(value);
  }

  fn add_one_specific(&mut self, key: String, value: String) {
    self.add_specific(key, vec![value]);
  }

  fn add_specific(&mut self, key: String, value: Vec<String>) {
    self.specific.insert(key, value);
  }
}

impl IntoResponse for Errors {
  fn into_response(self) -> Response {
    json!({"errors": &self}).to_string().into_response()
  }
}

// #[derive(EnumString)]
pub enum Object {
  Video,
}

#[derive(Error, Debug)]
pub enum HandlerError {
  #[error("Could not parse IDs in URL path.")]
  ParseUrlPath(#[from] PathRejection),
  #[error(transparent)]
  JsonRejection(#[from] JsonRejection),
  #[error(transparent)]
  Validation(#[from] ValidationErrors),
  #[error("Username must not be taken.")]
  UsernameTaken,
  #[error("Email Address must not be taken.")]
  EmailTaken,
  #[error("Login credentials are invalid.")]
  InvalidCredentials,
  #[error("User cannot be logged into.")]
  ImpossibleLogin,
  #[error("Video overwriting is not allowed once upload has completed.")]
  OverwriteVideo,
  #[error("Content-Range header must specify the content start, end, and size.")]
  InvalidRange,
  #[error("Content-Range size does not match body size.")]
  BodyRangeNoMatch,
  #[error("Content-Range header must specify the same total size for every request.")]
  InconsistentRange,
  #[error("Could not probe video details.")]
  ProbeVideo(#[from] ProbeError),
  #[error("Could not generate video thumbnail.")]
  GenerateThumbnail(#[from] GenerateThumbnailError),

  #[error("No user was logged in but a user is required: {0}.")]
  UserRequired(#[from] AuthError),

  #[error("You do not own this object.")]
  NotAnOwner,

  #[error("Could not find object.")]
  ObjectNotFound,

  #[error("Could not convert between integers.")]
  ConvertIntegers(#[from] TryFromIntError),
  #[error("Database transaction failed.")]
  Database(#[from] DbErr),
  #[error("Input / output operation failed.")]
  Io(#[from] std::io::Error),
  #[error("Could not create user.")]
  CreateUser(#[from] user::CreateError),
  #[error("Could not validate credentials.")]
  ValidateCredentials(#[from] user::ValidateCredentialsError),
  #[error("Could not encode JWT.")]
  EncodeJwt(jsonwebtoken::errors::Error),
}

impl HandlerError {
  fn into_generic(self, code: StatusCode) -> (StatusCode, Errors) {
    (code, Errors::generic(self.to_string()))
  }

  fn failed_validation(self, code: StatusCode, key: &str) -> (StatusCode, Errors) {
    let mut errors = FAILED_VALIDATION.clone();
    errors.add_one_specific(key.to_string(), self.to_string());
    (code, errors)
  }
}

fn format_error_messages(field: &str, errors: ValidationErrorsKind) -> Vec<String> {
  let title = field.to_case(Case::Title);

  match errors {
    ValidationErrorsKind::Field(errors) => errors
      .into_iter()
      .map(|e| match e.code.as_ref() {
        "email" => format!("{} must be a valid email address.", title),
        "must_match" => format!("{} must be identical to {}.", title, e.message.unwrap()),
        "length" => format!(
          "{} must be between {} and {} characters long (currently {}).",
          title,
          e.params.get("min").unwrap(),
          e.params.get("max").unwrap(),
          e.params.get("value").unwrap().as_str().unwrap().len(),
        ),
        code => unimplemented!(
          "error message is not implemented for message code '{}'",
          code
        ),
      })
      .collect(),
    ValidationErrorsKind::Struct(_) | ValidationErrorsKind::List(_) => {
      panic!("unexpected error type")
    }
  }
}

impl IntoResponse for HandlerError {
  fn into_response(self) -> Response {
    tracing::debug!("Encountered {:?}", &self);

    match self {
      HandlerError::ParseUrlPath(_) => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::JsonRejection(_) => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::Validation(validation_errors) => (StatusCode::BAD_REQUEST, {
        let mut errors = FAILED_VALIDATION.clone();
        for (k, v) in validation_errors.into_errors() {
          errors.add_specific(k.to_case(Case::Camel), format_error_messages(k, v));
        }
        errors
      }),
      HandlerError::UsernameTaken => self.failed_validation(StatusCode::BAD_REQUEST, "username"),
      HandlerError::EmailTaken => self.failed_validation(StatusCode::BAD_REQUEST, "emailAddress"),
      HandlerError::InvalidCredentials => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::ImpossibleLogin => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::OverwriteVideo => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::InvalidRange => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::BodyRangeNoMatch => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::InconsistentRange => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::ProbeVideo(_) => self.into_generic(StatusCode::BAD_REQUEST),
      HandlerError::GenerateThumbnail(_) => self.into_generic(StatusCode::BAD_REQUEST),

      HandlerError::UserRequired(_) => self.into_generic(StatusCode::UNAUTHORIZED),

      HandlerError::NotAnOwner => self.into_generic(StatusCode::FORBIDDEN),

      HandlerError::ObjectNotFound => self.into_generic(StatusCode::NOT_FOUND),

      HandlerError::ConvertIntegers(ref inner) => {
        error!("Convert integers: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
      HandlerError::Database(ref inner) => {
        error!("Database: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
      HandlerError::Io(ref inner) => {
        error!("IO failed: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
      HandlerError::CreateUser(ref inner) => {
        error!("Create user: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
      HandlerError::ValidateCredentials(ref inner) => {
        error!("Validate credentials: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
      HandlerError::EncodeJwt(ref inner) => {
        error!("Encode JWT: {}", inner);
        self.into_generic(StatusCode::INTERNAL_SERVER_ERROR)
      }
    }
    .into_response()
  }
}

#[derive(Debug, Clone, Copy, Default)]
pub struct ValidatedJson<T>(pub T);

#[async_trait]
impl<T, S> FromRequest<S> for ValidatedJson<T>
where
  T: DeserializeOwned + Validate,
  S: Send + Sync,
  Json<T>: FromRequest<S, Rejection = JsonRejection>,
{
  type Rejection = HandlerError;

  async fn from_request(req: Request, state: &S) -> Result<Self, Self::Rejection> {
    let Json(value) = Json::from_request(req, state).await?;
    value.validate()?;
    Ok(ValidatedJson(value))
  }
}
