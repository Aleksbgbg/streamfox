use std::path::{Path, PathBuf};
use tokio::fs::{self, File as TokioFile, OpenOptions};

#[derive(Debug)]
pub struct File {
  path: PathBuf,
}

impl File {
  pub fn from(path: String) -> Self {
    Self {
      path: PathBuf::from(path),
    }
  }

  pub fn path(&self) -> &Path {
    &self.path
  }

  pub async fn create(&self) -> std::io::Result<TokioFile> {
    if let Some(parent) = self.path.parent() {
      fs::create_dir_all(parent).await?;
    }

    TokioFile::create(&self.path).await
  }

  pub async fn open(&self) -> std::io::Result<TokioFile> {
    if self.path.exists() {
      OpenOptions::new().append(true).open(&self.path).await
    } else {
      self.create().await
    }
  }
}
