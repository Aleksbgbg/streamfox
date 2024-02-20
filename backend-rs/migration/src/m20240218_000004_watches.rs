use crate::m20240204_000001_users::User;
use crate::m20240218_000002_videos::Video;
use sea_orm_migration::prelude::*;

pub struct Migration;

impl MigrationName for Migration {
  fn name(&self) -> &str {
    "m_20240218_000004_watches"
  }
}

#[async_trait::async_trait]
impl MigrationTrait for Migration {
  async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .create_table(
        Table::create()
          .table(Watch::Table)
          .col(
            ColumnDef::new(Watch::UserId)
              .big_integer()
              .not_null()
              .primary_key(),
          )
          .col(ColumnDef::new(Watch::VideoId).big_integer().not_null())
          .col(
            ColumnDef::new(Watch::ViewId)
              .big_integer()
              .not_null()
              .unique_key(),
          )
          .col(
            ColumnDef::new(Watch::CreatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(Watch::UpdatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(Watch::StartedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(Watch::BytesStreamed)
              .big_integer()
              .not_null(),
          )
          .foreign_key(
            ForeignKey::create()
              .name("fk-watch-user")
              .from(Watch::Table, Watch::UserId)
              .to(User::Table, User::Id),
          )
          .foreign_key(
            ForeignKey::create()
              .name("fk-watch-video")
              .from(Watch::Table, Watch::VideoId)
              .to(Video::Table, Video::Id),
          )
          .to_owned(),
      )
      .await
  }

  async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .drop_table(Table::drop().table(Watch::Table).to_owned())
      .await
  }
}

#[derive(Iden)]
pub enum Watch {
  Table,
  UserId,
  VideoId,
  ViewId,
  CreatedAt,
  UpdatedAt,
  StartedAt,
  BytesStreamed,
}
