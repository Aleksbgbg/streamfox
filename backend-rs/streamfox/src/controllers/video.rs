use crate::controllers::errors::{HandlerError, Object};
use crate::controllers::user::UserResponse;
use crate::models::user::User;
use crate::models::video;
use crate::{codec, AppState, MainFsVar};
use axum::body::{Body, HttpBody};
use axum::extract::{Path, Request, State};
use axum::http::StatusCode;
use axum::middleware::Next;
use axum::response::Response;
use axum::{Extension, Json};
use axum_extra::headers::ContentRange;
use axum_extra::TypedHeader;
use cascade::cascade;
use chrono::{DateTime, Utc};
use entity::id::Id;
use entity::video::{ActiveModel, Status, Visibility};
use futures::TryStreamExt;
use sea_orm::{ActiveModelBehavior, ActiveValue};
use serde::Serialize;
use std::io::SeekFrom;
use std::sync::Arc;
use tokio::io::{self, AsyncSeekExt, AsyncWriteExt, BufWriter};
use tokio_util::io::StreamReader;

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct VideoResponse {
  id: Id,
  creator: UserResponse,
  duration_secs: u32,
  uploaded_at: DateTime<Utc>,
  name: String,
  description: String,
  visibility: Visibility,
  views: u64,
  likes: u64,
  dislikes: u64,
}

pub async fn get_videos(
  State(state): State<AppState>,
) -> Result<Json<Vec<VideoResponse>>, HandlerError> {
  Ok(Json(
    video::find_all(&state.connection)
      .await?
      .into_iter()
      .map(|(video, user)| {
        Ok(VideoResponse {
          id: video.id,
          creator: user.into(),
          duration_secs: video.duration_secs.try_into()?,
          uploaded_at: video.created_at.into(),
          name: video.name,
          description: video.description,
          visibility: video.visibility,
          views: video.views.try_into()?,
          likes: video.likes.try_into()?,
          dislikes: video.dislikes.try_into()?,
        })
      })
      .collect::<Result<_, HandlerError>>()?,
  ))
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
pub struct VideoCreatedResponse {
  id: Id,
  name: String,
  description: String,
  visibility: Visibility,
}

pub async fn create_video(
  State(state): State<AppState>,
  Extension(user): Extension<Arc<User>>,
) -> Result<(StatusCode, Json<VideoCreatedResponse>), HandlerError> {
  let video = video::create(&state.connection, &state.snowflakes, &user).await?;
  Ok((
    StatusCode::CREATED,
    Json(VideoCreatedResponse {
      id: video.id,
      name: video.name,
      description: video.description,
      visibility: video.visibility,
    }),
  ))
}

pub async fn extract(
  State(state): State<AppState>,
  Path(video_id): Path<Id>,
  mut request: Request,
  next: Next,
) -> Result<Response, HandlerError> {
  let video = video::find(&state.connection, video_id)
    .await?
    .ok_or(HandlerError::ObjectNotFound)?;

  request.extensions_mut().insert(Arc::new(video));

  Ok(next.run(request).await)
}

pub async fn require_owner(
  Extension(user): Extension<Arc<User>>,
  Extension(video): Extension<Arc<(entity::video::Model, User)>>,
  request: Request,
  next: Next,
) -> Result<Response, HandlerError> {
  if video.0.creator_id == user.id {
    Ok(next.run(request).await)
  } else {
    Err(HandlerError::NotAnOwner)
  }
}

pub async fn upload_video(
  State(state): State<AppState>,
  Extension(video): Extension<Arc<(entity::video::Model, User)>>,
  TypedHeader(range): TypedHeader<ContentRange>,
  body: Body,
) -> Result<StatusCode, HandlerError> {
  let video = &video.0;

  if video.status > Status::Uploading {
    return Err(HandlerError::OverwriteVideo);
  }

  let data_stream = body.into_data_stream();

  let (start, end) = range.bytes_range().ok_or(HandlerError::InvalidRange)?;
  let current_len = end - start + 1;
  let total_len = range.bytes_len().ok_or(HandlerError::InvalidRange)?;

  if (current_len < data_stream.size_hint().lower())
    || data_stream
      .size_hint()
      .upper()
      .is_some_and(|real_len| current_len != real_len)
  {
    return Err(HandlerError::BodyRangeNoMatch);
  }

  let mut active = ActiveModel::new();
  active.id = ActiveValue::set(video.id);

  if start == 0 {
    active.status = ActiveValue::set(Status::Uploading);
    active.size_bytes = ActiveValue::set(total_len.try_into()?);
  } else if total_len != (video.size_bytes as u64) {
    return Err(HandlerError::InconsistentRange);
  }

  let fs = cascade! {
    state.fs.clone();
    ..set(MainFsVar::VideoId, video.id);
  };

  {
    let mut file = fs.video_stream().open().await?;
    file.seek(SeekFrom::Start(start)).await?;

    let mut reader =
      StreamReader::new(data_stream.map_err(|err| io::Error::new(io::ErrorKind::Other, err)));
    let mut writer = BufWriter::new(file);

    io::copy(&mut reader, &mut writer).await?;
    writer.flush().await?;
  }

  if (end + 1) < total_len {
    video::update(&state.connection, active).await?;
    return Ok(StatusCode::ACCEPTED);
  }

  let probe = codec::probe(&fs).await?;
  codec::generate_thumbnail(&fs, &probe).await?;

  active.mime_type = ActiveValue::set(probe.mime_type);
  active.duration_secs = ActiveValue::set(probe.duration.num_seconds().try_into()?);
  active.status = ActiveValue::set(Status::Complete);
  video::update(&state.connection, active).await?;

  Ok(StatusCode::NO_CONTENT)
}
