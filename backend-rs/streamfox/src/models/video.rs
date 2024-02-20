use crate::models::user::User;
use chrono::{DateTime, Utc};
use entity::id::Id;
use entity::video::Visibility;
use entity::{user, video, view};
use sea_orm::{
  ColumnTrait, DatabaseConnection, DbErr, EntityTrait, FromQueryResult, JoinType, QuerySelect,
};

#[derive(FromQueryResult)]
pub struct VideoResult {
  pub id: Id,
  pub duration_secs: u32,
  pub uploaded_at: DateTime<Utc>,
  pub name: String,
  pub description: String,
  pub visibility: Visibility,
  pub views: u64,
  pub likes: u64,
  pub dislikes: u64,
}

pub async fn find_all(connection: &DatabaseConnection) -> Result<Vec<(VideoResult, User)>, DbErr> {
  video::Entity::find()
    .find_also_related(user::Entity)
    .join_rev(
      JoinType::InnerJoin,
      view::Entity::belongs_to(video::Entity)
        .from(view::Column::VideoId)
        .to(video::Column::Id)
        .into(),
    )
    .column_as(view::Column::Id.count(), "views")
    .group_by(video::Column::Id)
    .group_by(user::Column::Id)
    .into_model::<VideoResult, user::Model>()
    .all(connection)
    .await?
    .into_iter()
    .map(|(video, creator)| {
      Ok((
        video,
        creator
          .map(Into::into)
          .ok_or(DbErr::Custom("video creator was empty".into()))?,
      ))
    })
    .collect()
}
