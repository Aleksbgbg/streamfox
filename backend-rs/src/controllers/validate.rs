use crate::controllers::errors::{self, Errors};
use axum::extract::rejection::{FormRejection, JsonRejection};
use axum::extract::{FromRequest, Request};
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use axum::{async_trait, Form, Json};
use convert_case::{Case, Casing};
use serde::de::DeserializeOwned;
use thiserror::Error;
use validator::{Validate, ValidationErrors, ValidationErrorsKind};

#[derive(Debug, Error)]
pub enum ValidationError {
  #[error(transparent)]
  JsonRejection(#[from] JsonRejection),
  #[error(transparent)]
  ValidationError(#[from] ValidationErrors),
}

fn format_messages(field: &str, errors: ValidationErrorsKind) -> Vec<String> {
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

impl IntoResponse for ValidationError {
  fn into_response(self) -> Response {
    (
      StatusCode::BAD_REQUEST,
      match self {
        ValidationError::JsonRejection(_) => Errors::generic(self.to_string()),
        ValidationError::ValidationError(validation_errors) => {
          let mut errors = errors::FAILED_VALIDATION.clone();
          for (k, v) in validation_errors.into_errors() {
            errors.add_specific(k.to_case(Case::Camel), format_messages(k, v));
          }
          errors
        }
      },
    )
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
  Form<T>: FromRequest<S, Rejection = FormRejection>,
{
  type Rejection = ValidationError;

  async fn from_request(req: Request, state: &S) -> Result<Self, Self::Rejection> {
    let Json(value) = Json::<T>::from_request(req, state).await?;
    value.validate()?;
    Ok(ValidatedJson(value))
  }
}
