use crate::models::id::Id;
use sea_orm::entity::prelude::*;

#[derive(Clone, Debug, PartialEq, DeriveEntityModel, Eq)]
#[sea_orm(table_name = "watch")]
pub struct Model {
  #[sea_orm(primary_key, auto_increment = false)]
  pub user_id: Id,
  pub video_id: Id,
  #[sea_orm(unique)]
  pub view_id: Id,
  pub created_at: DateTimeWithTimeZone,
  pub updated_at: DateTimeWithTimeZone,
  pub started_at: DateTimeWithTimeZone,
  pub bytes_streamed: i64,
}

#[derive(Copy, Clone, Debug, EnumIter, DeriveRelation)]
pub enum Relation {
  #[sea_orm(
    belongs_to = "super::user::Entity",
    from = "Column::UserId",
    to = "super::user::Column::Id",
    on_update = "NoAction",
    on_delete = "NoAction"
  )]
  User,
  #[sea_orm(
    belongs_to = "super::video::Entity",
    from = "Column::VideoId",
    to = "super::video::Column::Id",
    on_update = "NoAction",
    on_delete = "NoAction"
  )]
  Video,
}

impl Related<super::user::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::User.def()
  }
}

impl Related<super::video::Entity> for Entity {
  fn to() -> RelationDef {
    Relation::Video.def()
  }
}

impl ActiveModelBehavior for ActiveModel {}
