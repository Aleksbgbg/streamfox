use crate::{
  m20240204_000001_users, m20240218_000002_videos, m20240218_000003_views, m20240218_000004_watches,
};
use sea_orm_migration::prelude::*;

pub struct Migrator;

#[async_trait::async_trait]
impl MigratorTrait for Migrator {
  fn migrations() -> Vec<Box<dyn MigrationTrait>> {
    vec![
      Box::new(m20240204_000001_users::Migration),
      Box::new(m20240218_000002_videos::Migration),
      Box::new(m20240218_000003_views::Migration),
      Box::new(m20240218_000004_watches::Migration),
    ]
  }
}
