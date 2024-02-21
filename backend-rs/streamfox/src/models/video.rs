use crate::models::base;
use crate::models::user::User;
use crate::Snowflakes;
use chrono::{DateTime, Local, Utc};
use entity::id::Id;
use entity::video::{Status, Visibility};
use entity::{user, video, view};
use sea_orm::{
  ActiveModelTrait, ActiveValue, ColumnTrait, DatabaseConnection, DbErr, EntityTrait,
  FromQueryResult, JoinType, QuerySelect,
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

pub struct Video {
  pub id: Id,
  pub name: String,
  pub description: String,
  pub visibility: Visibility,
}

impl From<video::Model> for Video {
  fn from(value: video::Model) -> Self {
    Video {
      id: value.id,
      name: value.name,
      description: value.description,
      visibility: value.visibility,
    }
  }
}

pub async fn create(
  connection: &DatabaseConnection,
  snowflakes: &Snowflakes,
  creator: User,
) -> Result<Video, DbErr> {
  let time = Local::now().fixed_offset();
  Ok(
    video::ActiveModel {
      id: ActiveValue::set(base::new_id(&snowflakes.video_snowflake).await),
      created_at: ActiveValue::set(time),
      updated_at: ActiveValue::set(time),
      status: ActiveValue::set(Status::Created),
      mime_type: ActiveValue::set(mime::APPLICATION_OCTET_STREAM.to_string()),
      duration_secs: ActiveValue::set(0),
      size_bytes: ActiveValue::set(0),
      subtitles_extracted: ActiveValue::set(false),
      creator_id: ActiveValue::set(creator.id),
      name: ActiveValue::set("Untitled Video".into()),
      description: ActiveValue::set(String::new()),
      visibility: ActiveValue::set(Visibility::Public),
    }
    .insert(connection)
    .await?
    .into(),
  )
}
