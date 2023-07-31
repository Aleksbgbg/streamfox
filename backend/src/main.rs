use crate::models::{base, users};
use dotenv::dotenv;
use rocket::serde::json::Json;
use rocket::serde::Deserialize;

mod models;

#[macro_use]
extern crate rocket;

#[derive(Deserialize)]
struct Login<'r> {
  username: &'r str,
  password: &'r str,
}

#[post("/auth/login", data = "<input>")]
async fn index(input: Json<Login<'_>>) {
  let user = users::find_user(input.username).await.unwrap();
  println!(
    "Hello, {}! (password {}, email {})",
    input.username,
    input.password,
    user.email_address.unwrap()
  );
}

#[launch]
async fn rocket() -> _ {
  dotenv().ok();

  base::init_db().await;

  rocket::build().mount("/api", routes![index])
}
