use crate::models::migrations::m20240204_000001_users::User;
use sea_orm_migration::prelude::*;

pub struct Migration;

impl MigrationName for Migration {
  fn name(&self) -> &str {
    "m_20240218_000002_videos"
  }
}

#[async_trait::async_trait]
impl MigrationTrait for Migration {
  async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .create_table(
        Table::create()
          .table(Video::Table)
          .col(
            ColumnDef::new(Video::Id)
              .big_integer()
              .not_null()
              .primary_key(),
          )
          .col(
            ColumnDef::new(Video::CreatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(Video::UpdatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(ColumnDef::new(Video::Status).small_integer().not_null())
          .col(ColumnDef::new(Video::MimeType).text().not_null())
          .col(ColumnDef::new(Video::DurationSecs).integer().not_null())
          .col(ColumnDef::new(Video::SizeBytes).big_integer().not_null())
          .col(
            ColumnDef::new(Video::SubtitlesExtracted)
              .boolean()
              .not_null(),
          )
          .col(ColumnDef::new(Video::CreatorId).big_integer().not_null())
          .col(ColumnDef::new(Video::Name).text().not_null())
          .col(ColumnDef::new(Video::Description).text().not_null())
          .col(ColumnDef::new(Video::Visibility).small_integer().not_null())
          .foreign_key(
            ForeignKey::create()
              .name("fk-video-creator")
              .from(Video::Table, Video::CreatorId)
              .to(User::Table, User::Id),
          )
          .to_owned(),
      )
      .await
  }

  async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .drop_table(Table::drop().table(Video::Table).to_owned())
      .await
  }
}

#[derive(Iden)]
pub enum Video {
  Table,
  Id,
  CreatedAt,
  UpdatedAt,
  Status,
  MimeType,
  DurationSecs,
  SizeBytes,
  SubtitlesExtracted,
  CreatorId,
  Name,
  Description,
  Visibility,
}
