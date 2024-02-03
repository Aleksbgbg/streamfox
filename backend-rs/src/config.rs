use serde::{Deserialize, Serialize};
use toml_env::Args;

#[derive(Serialize, Deserialize)]
pub struct Config {
  pub app: App,
}

#[derive(Serialize, Deserialize)]
pub struct App {
  pub host: [u8; 4],
  pub port: u16,
}

pub fn load() -> Config {
  toml_env::initialize(Args {
    config_variable_name: "config",
    ..Default::default()
  })
  .expect("could not initialize toml_env")
  .expect("could not load config")
}
