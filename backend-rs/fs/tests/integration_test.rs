use cascade::cascade;
use fs::filesystem;
use std::path::PathBuf;

#[test]
fn can_be_called() {
  filesystem!(TestFs, "");
}

#[test]
fn generates_file_method() {
  filesystem!(TestFs, "filename #file");

  assert_eq!(PathBuf::from("filename"), TestFs::new().file().path());
}

#[test]
fn generates_file_method_resolving_vars() {
  filesystem!(
    TestFs,
    r"
<Parent>
  <Filename>.<Extension> #file
"
  );

  assert_eq!(
    PathBuf::from("parent/filename.txt"),
    cascade! {
    TestFs::new();
      ..set(TestFsVar::Parent, "parent");
      ..set(TestFsVar::Filename, "filename");
      ..set(TestFsVar::Extension, "txt");
    }
    .file()
    .path()
  );
}
