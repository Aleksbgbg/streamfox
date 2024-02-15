use std::path::{Path, PathBuf};

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
}
