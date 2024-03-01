use crate::MainFs;
use chrono::Duration;
use lazy_static::lazy_static;
use serde::Deserialize;
use serde_this_or_that::as_f64;
use std::collections::HashMap;
use std::num::ParseFloatError;
use std::string::FromUtf8Error;
use thiserror::Error;
use tokio::process::Command;

lazy_static! {
  static ref FORMAT_TO_MIME: HashMap<&'static str, &'static str> = HashMap::from([
    ("3dostr", "application/vnd.pg.format"),
    ("3g2", "video/3gpp2"),
    ("3gp", "video/3gpp"),
    ("4xm", "audio/x-adpcm"),
    ("a64", "application/octet-stream"),
    ("aa", "application/octet-stream"),
    ("aac", "audio/aac"),
    ("ac3", "audio/x-ac3"),
    ("acm", "application/octet-stream"),
    ("adts", "audio/aac"),
    ("aiff", "audio/aiff"),
    ("amr", "audio/amr"),
    ("apng", "image/png"),
    ("asf", "video/x-ms-asf"),
    ("asf_stream", "video/x-ms-asf"),
    ("ass", "text/x-ass"),
    ("au", "audio/basic"),
    ("avi", "video/x-msvideo"),
    ("avm2", "application/x-shockwave-flash"),
    ("bin", "application/octet-stream"),
    ("bit", "audio/bit"),
    ("caf", "audio/x-caf"),
    ("dts", "audio/x-dca"),
    ("dvd", "video/mpeg"),
    ("eac3", "audio/x-eac3"),
    ("f4v", "application/f4v"),
    ("flac", "audio/x-flac"),
    ("flv", "video/x-flv"),
    ("g722", "audio/G722"),
    ("g723_1", "audio/g723"),
    ("gif", "image/gif"),
    ("gsm", "audio/x-gsm"),
    ("h261", "video/x-h261"),
    ("h263", "video/x-h263"),
    ("hls", "application/x-mpegURL"),
    ("hls,applehttp", "application/x-mpegURL"),
    ("ico", "image/vnd.microsoft.icon"),
    ("ilbc", "audio/iLBC"),
    ("ipod", "video/mp4"),
    ("ismv", "video/mp4"),
    ("jacosub", "text/x-jacosub"),
    ("jpeg_pipe", "image/jpeg"),
    ("jpegls_pipe", "image/jpeg"),
    ("latm", "audio/MP4A-LATM"),
    ("live_flv", "video/x-flv"),
    ("m4v", "video/x-m4v"),
    ("matroska", "video/x-matroska"),
    ("matroska,webm", "video/webm"),
    ("microdvd", "text/x-microdvd"),
    ("mjpeg", "video/x-mjpeg"),
    ("mjpeg_2000", "video/x-mjpeg"),
    ("mmf", "application/vnd.smaf"),
    ("mov,mp4,m4a,3gp,3g2,mj2", "video/mp4"),
    ("mp2", "audio/mpeg"),
    ("mp3", "audio/mpeg"),
    ("mp4", "video/mp4"),
    ("mpeg", "video/mpeg"),
    ("mpeg1video", "video/mpeg"),
    ("mpeg2video", "video/mpeg"),
    ("mpegts", "video/MP2T"),
    ("mpegtsraw", "video/MP2T"),
    ("mpegvideo", "video/mpeg"),
    ("mpjpeg", "multipart/x-mixed-replace"),
    ("mxf", "application/mxf"),
    ("mxf_d10", "application/mxf"),
    ("mxf_opatom", "application/mxf"),
    ("nut", "video/x-nut"),
    ("oga", "audio/ogg"),
    ("ogg", "application/ogg"),
    ("ogv", "video/ogg"),
    ("oma", "audio/x-oma"),
    ("opus", "audio/ogg"),
    ("rm", "application/vnd.rn-realmedia"),
    ("singlejpeg", "image/jpeg"),
    ("smjpeg", "image/jpeg"),
    ("spx", "audio/ogg"),
    ("srt", "application/x-subrip"),
    ("sup", "application/x-pgs"),
    ("svcd", "video/mpeg"),
    ("swf", "application/x-shockwave-flash"),
    ("tta", "audio/x-tta"),
    ("vcd", "video/mpeg"),
    ("vob", "video/mpeg"),
    ("voc", "audio/x-voc"),
    ("wav", "audio/x-wav"),
    ("webm", "video/webm"),
    ("webm_chunk", "video/webm"),
    ("webm_dash_manifest", "application/xml"),
    ("webp", "image/webp"),
    ("webvtt", "text/vtt"),
    ("wv", "audio/x-wavpack"),
  ]);
}

#[derive(Deserialize)]
struct ProbeOutput<'a> {
  #[serde(borrow)]
  format: Format<'a>,
}

#[derive(Deserialize)]
struct Format<'a> {
  #[serde(borrow, rename = "format_name")]
  name: &'a str,
  #[serde(deserialize_with = "as_f64")]
  duration: f64,
}

pub struct Probe {
  pub mime_type: String,
  pub duration: Duration,
}

#[derive(Error, Debug)]
pub enum ProbeError {
  #[error("could not run ffprobe: `{0}`")]
  Command(#[from] std::io::Error),
  #[error("could not parse ffprobe error output as UTF-8: `{0}`")]
  ParseErrorStream(#[from] FromUtf8Error),
  #[error("ffprobe failed to run: `{0}`")]
  FfProbeFailed(String),
  #[error("could not parse probe output: `{0}`")]
  ParseJson(#[from] serde_json::Error),
  #[error("could not parse duration float: `{0}`")]
  ParseFloat(#[from] ParseFloatError),
  #[error("format `{0}` is not supported")]
  UnsupportedFormat(String),
}

pub async fn probe(fs: &MainFs) -> Result<Probe, ProbeError> {
  let output = Command::new("ffprobe")
    .arg("-loglevel")
    .arg("error")
    .arg("-show_format")
    .arg("-print_format")
    .arg("json")
    .arg(fs.video_stream().path().as_os_str())
    .output()
    .await?;

  if !output.status.success() {
    return Err(ProbeError::FfProbeFailed(String::from_utf8(output.stderr)?));
  }

  let probe: ProbeOutput = serde_json::from_slice(&output.stdout)?;
  let format = probe.format;

  if let Some(mime_type) = FORMAT_TO_MIME.get(format.name) {
    Ok(Probe {
      mime_type: (*mime_type).into(),
      duration: Duration::seconds(format.duration.ceil() as i64),
    })
  } else {
    Err(ProbeError::UnsupportedFormat(format.name.into()))
  }
}

#[derive(Error, Debug)]
pub enum GenerateThumbnailError {
  #[error("could not run ffmpeg: `{0}`")]
  Command(#[from] std::io::Error),
  #[error("could not parse ffmpeg error output as UTF-8: `{0}`")]
  ParseErrorStream(#[from] FromUtf8Error),
  #[error("ffmpeg failed to run: `{0}`")]
  FfMpegFailed(String),
}

pub async fn generate_thumbnail(fs: &MainFs, probe: &Probe) -> Result<(), GenerateThumbnailError> {
  const SEEK_FRACTION: f64 = 0.2;

  let output = Command::new("ffmpeg")
    .arg("-loglevel")
    .arg("error")
    .arg("-y")
    .arg("-ss")
    .arg(
      (SEEK_FRACTION * (probe.duration.num_seconds() as f64))
        .floor()
        .to_string(),
    )
    .arg("-i")
    .arg(fs.video_stream().path().as_os_str())
    .arg("-vframes")
    .arg("1")
    .arg("-q:v")
    .arg("2")
    .arg("-vf")
    .arg("scale=416:234:force_original_aspect_ratio=decrease,pad=416:234:-1:-1:color=black")
    .arg("-f")
    .arg("mjpeg")
    .arg(fs.video_thumbnail().path().as_os_str())
    .output()
    .await?;

  if !output.status.success() {
    return Err(GenerateThumbnailError::FfMpegFailed(String::from_utf8(
      output.stderr,
    )?));
  }

  Ok(())
}
