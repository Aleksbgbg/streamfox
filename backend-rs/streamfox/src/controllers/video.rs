use crate::controllers::errors::HandlerError;
use crate::controllers::user::UserResponse;
use crate::models::video;
use crate::AppState;
use axum::extract::State;
use axum::Json;
use chrono::{DateTime, Utc};
use entity::id::Id;
use entity::video::Visibility;
use serde::Serialize;

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
      .map(|(video, user)| VideoResponse {
        id: video.id,
        creator: user.into(),
        duration_secs: video.duration_secs,
        uploaded_at: video.uploaded_at,
        name: video.name,
        description: video.description,
        visibility: video.visibility,
        views: video.views,
        likes: 0,
        dislikes: 0,
      })
      .collect(),
  ))
}