use crate::models::base;
use crate::models::user::User;
use crate::Snowflakes;
use chrono::Local;
use entity::id::Id;
use entity::video::{Status, Visibility};
use entity::{user, video, view};
use sea_orm::prelude::DateTimeWithTimeZone;
use sea_orm::sea_query::Expr;
use sea_orm::{
  ActiveModelTrait, ActiveValue, ColumnTrait, DatabaseConnection, DbErr, EntityTrait,
  FromQueryResult, JoinType, QueryFilter, QuerySelect,
};

#[derive(FromQueryResult)]
pub struct VideoResult {
  pub id: Id,
  pub duration_secs: i32,
  pub created_at: DateTimeWithTimeZone,
  pub name: String,
  pub description: String,
  pub visibility: Visibility,
  pub views: i64,
  pub likes: i32,
  pub dislikes: i32,
}

pub async fn find_all(connection: &DatabaseConnection) -> Result<Vec<(VideoResult, User)>, DbErr> {
  video::Entity::find()
    .filter(video::Column::Status.gte(Status::Complete))
    .filter(video::Column::Visibility.gte(Visibility::Public))
    .join_rev(
      JoinType::LeftJoin,
      view::Entity::belongs_to(video::Entity)
        .from(view::Column::VideoId)
        .to(video::Column::Id)
        .into(),
    )
    .column_as(view::Column::Id.count(), "views")
    .expr_as_(Expr::value(0), "likes")
    .expr_as_(Expr::value(0), "dislikes")
    .find_also_related(user::Entity)
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
          .ok_or_else(|| DbErr::Custom("video creator was empty".into()))?,
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
  creator: &User,
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
