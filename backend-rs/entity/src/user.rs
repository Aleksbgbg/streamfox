use crate::id::Id;
use sea_orm::entity::prelude::*;

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "user")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub id: Id,
  pub created_at: DateTimeWithTimeZone,
  pub updated_at: DateTimeWithTimeZone,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub username: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub canonical_username: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub email_address: Option<String>,
  #[sea_orm(column_type = "Text", nullable, unique)]
  pub canonical_email_address: Option<String>,
  #[sea_orm(column_type = "Text", nullable)]
  pub password: Option<String>,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
  #[sea_orm(has_many = "super::video::Entity")]
  Video,
  #[sea_orm(has_many = "super::view::Entity")]
  View,
  #[sea_orm(has_many = "super::watch::Entity")]
  Watch,
}

impl Related<super::video::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::Video.def()
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
