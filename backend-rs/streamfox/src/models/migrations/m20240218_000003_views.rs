use crate::models::migrations::m20240204_000001_users::User;
use crate::models::migrations::m20240218_000002_videos::Video;
use sea_orm_migration::prelude::*;

pub struct Migration;

impl MigrationName for Migration {
  fn name(&self) -> &str {
    "m_20240218_000003_views"
  }
}

#[async_trait::async_trait]
impl MigrationTrait for Migration {
  async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .create_table(
        Table::create()
          .table(View::Table)
          .col(
            ColumnDef::new(View::Id)
              .big_integer()
              .not_null()
              .primary_key(),
          )
          .col(
            ColumnDef::new(View::CreatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(View::UpdatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(ColumnDef::new(View::UserId).big_integer().not_null())
          .col(ColumnDef::new(View::VideoId).big_integer().not_null())
          .col(
            ColumnDef::new(View::StartedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(ColumnDef::new(View::BytesStreamed).big_integer().not_null())
          .foreign_key(
            ForeignKey::create()
              .name("fk-view-user")
              .from(View::Table, View::UserId)
              .to(User::Table, User::Id),
          )
          .foreign_key(
            ForeignKey::create()
              .name("fk-view-video")
              .from(View::Table, View::VideoId)
              .to(Video::Table, Video::Id),
          )
          .to_owned(),
      )
      .await
  }

  async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .drop_table(Table::drop().table(View::Table).to_owned())
      .await
  }
}

#[derive(Iden)]
pub enum View {
  Table,
  Id,
  CreatedAt,
  UpdatedAt,
  UserId,
  VideoId,
  StartedAt,
  BytesStreamed,
}
