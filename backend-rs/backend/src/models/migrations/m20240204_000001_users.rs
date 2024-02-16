use sea_orm_migration::prelude::*;

pub struct Migration;

impl MigrationName for Migration {
  fn name(&self) -> &str {
    "m_20240204_000001_users"
  }
}

#[async_trait::async_trait]
impl MigrationTrait for Migration {
  async fn up(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .create_table(
        Table::create()
          .table(User::Table)
          .col(
            ColumnDef::new(User::Id)
              .big_integer()
              .not_null()
              .primary_key(),
          )
          .col(
            ColumnDef::new(User::CreatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(
            ColumnDef::new(User::UpdatedAt)
              .timestamp_with_time_zone()
              .not_null(),
          )
          .col(ColumnDef::new(User::Username).text().unique_key())
          .col(ColumnDef::new(User::CanonicalUsername).text().unique_key())
          .col(ColumnDef::new(User::EmailAddress).text().unique_key())
          .col(
            ColumnDef::new(User::CanonicalEmailAddress)
              .text()
              .unique_key(),
          )
          .col(ColumnDef::new(User::Password).text())
          .to_owned(),
      )
      .await
  }

  async fn down(&self, manager: &SchemaManager) -> Result<(), DbErr> {
    manager
      .drop_table(Table::drop().table(User::Table).to_owned())
      .await
  }
}

#[derive(Iden)]
pub enum User {
  Table,
  Id,
  CreatedAt,
  UpdatedAt,
  Username,
  CanonicalUsername,
  EmailAddress,
  CanonicalEmailAddress,
  Password,
}
