use crate::id::Id;
use sea_orm::entity::prelude::*;
use serde::{Deserialize, Serialize};

#[derive(
  Clone, Debug, PartialEq, Eq, PartialOrd, EnumIter, DeriveActiveEnum, Serialize, Deserialize,
)]
#[sea_orm(rs_type = "i16", db_type = "SmallInteger")]
#[serde(untagged)]
pub enum Status {
  Created = 0,
  Uploading = 1,
  Processing = 2,
  Complete = 3,
}

#[derive(Clone, Debug, PartialEq, Eq, EnumIter, DeriveActiveEnum, Serialize, Deserialize)]
#[sea_orm(rs_type = "i16", db_type = "SmallInteger")]
#[serde(untagged)]
pub enum Visibility {
  Private = 0,
  Unlisted = 1,
  Public = 2,
}

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "video")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub id: Id,
  pub created_at: DateTimeWithTimeZone,
  pub updated_at: DateTimeWithTimeZone,
  pub status: Status,
  #[sea_orm(column_type = "Text")]
  pub mime_type: String,
  pub duration_secs: i32,
  pub size_bytes: i64,
  pub subtitles_extracted: bool,
  pub creator_id: Id,
  #[sea_orm(column_type = "Text")]
  pub name: String,
  #[sea_orm(column_type = "Text")]
  pub description: String,
  pub visibility: Visibility,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
  #[sea_orm(
    belongs_to = "super::user::Entity",
    from = "Column::CreatorId",
    to = "super::user::Column::Id",
    on_update = "NoAction",
    on_delete = "NoAction"
  )]
  User,
  #[sea_orm(has_many = "super::view::Entity")]
  View,
  #[sea_orm(has_many = "super::watch::Entity")]
  Watch,
}

impl Related<super::user::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::User.def()
  }
}

impl Related<super::view::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::View.def()
  }
}

impl Related<super::watch::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::Watch.def()
  }
}

impl ActiveModelBehavior for ActiveModel {}
