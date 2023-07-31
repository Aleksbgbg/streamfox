use once_cell::sync::OnceCell;
use sea_orm::{Database, DatabaseConnection};
use std::env;

pub static DB: OnceCell<DatabaseConnection> = OnceCell::new();

pub fn get_db() -> &'static DatabaseConnection {
  DB.get().unwrap()
}

pub async fn init_db() {
  let host = env::var("DB_HOST").unwrap();
  let port = env::var("DB_PORT").unwrap();
  let user = env::var("DB_USER").unwrap();
  let password = env::var("DB_PASSWORD").unwrap();
  let name = env::var("DB_NAME").unwrap();

  let db = Database::connect(format!("postgres://{user}:{password}@{host}:{port}/{name}"))
    .await
    .expect("could not connect to database");

  DB.set(db).unwrap();
}
