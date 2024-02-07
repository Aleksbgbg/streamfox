use crate::models::user;
use axum::http::StatusCode;
use axum::response::{IntoResponse, Response};
use lazy_static::lazy_static;
use sea_orm::DbErr;
use serde::Serialize;
use serde_json::json;
use std::collections::HashMap;
use thiserror::Error;

lazy_static! {
  pub static ref FAILED_VALIDATION: Errors =
    Errors::generic("Some inputs failed validation.".to_string());
}

#[derive(Serialize, Default, Clone)]
pub struct Errors {
  generic: Vec<String>,
  specific: HashMap<String, Vec<String>>,
}

impl Errors {
  pub fn generic(value: String) -> Self {
    let mut errors = Self::default();
    errors.add_generic(value);
    errors
  }

  pub fn add_generic(&mut self, value: String) {
    self.generic.push(value);
  }

  pub fn add_one_specific(&mut self, key: String, value: String) {
    self.add_specific(key, vec![value]);
  }

  pub fn add_specific(&mut self, key: String, value: Vec<String>) {
    self.specific.insert(key, value);
  }
}

impl IntoResponse for Errors {
  fn into_response(self) -> Response {
    json!({"errors": &self}).to_string().into_response()
  }
}

#[derive(Error, Debug)]
pub enum HandlerError {
  #[error("Username must not be taken.")]
  UsernameTaken,
  #[error("Email Address must not be taken.")]
  EmailTaken,

  #[error("Database transaction failed.")]
  Database(#[from] DbErr),
  #[error("Could not create user.")]
  CreateUser(#[from] user::CreateError),
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

impl IntoResponse for HandlerError {
  fn into_response(self) -> Response {
    match self {
      HandlerError::UsernameTaken => self.failed_validation(StatusCode::BAD_REQUEST, "username"),
      HandlerError::EmailTaken => self.failed_validation(StatusCode::BAD_REQUEST, "emailAddress"),
      HandlerError::Database(_) => self.into_generic(StatusCode::INTERNAL_SERVER_ERROR),
      HandlerError::CreateUser(_) => self.into_generic(StatusCode::INTERNAL_SERVER_ERROR),
    }
    .into_response()
  }
}
