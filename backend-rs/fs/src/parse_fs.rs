use std::collections::HashSet;
use std::mem;

const DEFAULT_INDENT: usize = 2;

#[cfg_attr(test, derive(Debug, PartialEq, Eq, Default))]
pub(crate) struct FsPath {
  pub name: String,
  pub path_format: String,
  pub vars: Vec<String>,
}

#[cfg_attr(test, derive(Debug, PartialEq, Eq, Default))]
pub(crate) struct ParsedFs {
  pub paths: Vec<FsPath>,
  pub unique_vars: HashSet<String>,
}

#[derive(PartialEq, Eq)]
enum Context {
  Indent,
  Path,
  Var,
  Name,
}

pub(crate) fn parse_fs(tree: &str) -> ParsedFs {
  let mut level = 0;

  let mut name = String::new();
  let mut path_format = String::new();
  let mut path_segments_at_level = vec![0];
  let mut var = String::new();
  let mut vars = Vec::new();
  let mut vars_at_level = vec![0];

  let mut context = Context::Path;
  let mut push = &mut path_format;

  let mut paths = Vec::new();
  let mut unique_vars = HashSet::new();

  for char in tree.chars() {
    if char.is_whitespace() {
      if context == Context::Indent {
        level += 1;
      } else if char == '\n' {
        if context == Context::Path {
          if !push.is_empty() {
            push.push('/');
            path_segments_at_level.push(push.len());
            vars_at_level.push(vars.len());
          }
        } else if context == Context::Name {
          try_finalize_current(&mut name, &path_format, &vars, &mut paths);

          context = Context::Indent;
          push = &mut path_format;
        } else {
          unreachable!();
        }
      }

      continue;
    }

    if char == '#' {
      context = Context::Name;
      push = &mut name;
      continue;
    }

    // Indent finished
    if context == Context::Indent {
      let indentations = level / DEFAULT_INDENT;
      let desired_level = indentations + 1; // Element at index 0 causes a full truncate, always keep it in the list

      path_segments_at_level.truncate(desired_level);
      push.truncate(*path_segments_at_level.last().unwrap());

      vars_at_level.truncate(desired_level);
      vars.truncate(*vars_at_level.last().unwrap());

      level = 0;
      context = Context::Path;
    }

    if char == '<' {
      context = Context::Var;
      push = &mut var;
      continue;
    }
    if char == '>' {
      context = Context::Path;
      push = &mut path_format;
      push.push_str("{}");

      let var = mem::take(&mut var);
      vars.push(var.clone());
      unique_vars.insert(var);

      continue;
    }

    push.push(char);
  }
  try_finalize_current(&mut name, &path_format, &vars, &mut paths);

  ParsedFs { paths, unique_vars }
}

fn try_finalize_current(
  name: &mut String,
  path_format: &str,
  vars: &[String],
  paths: &mut Vec<FsPath>,
) {
  if name.is_empty() {
    return;
  }

  paths.push(FsPath {
    name: mem::take(name),
    path_format: path_format.to_owned(),
    vars: vars.to_owned(),
  });
}

#[cfg(test)]
mod tests {
  use super::*;

  #[test]
  fn parse_empty() {
    let result = parse_fs("");

    assert_eq!(result, Default::default());
  }

  #[test]
  fn parse_single_file() {
    let result = parse_fs("filename #file");

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "filename".into(),
          vars: Default::default(),
        }],
        unique_vars: Default::default(),
      }
    );
  }

  #[test]
  fn parse_nested_file() {
    let result = parse_fs(
      r"
parent
  filename #file
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "parent/filename".into(),
          vars: Default::default(),
        }],
        unique_vars: Default::default(),
      }
    );
  }

  #[test]
  fn parse_multiple_nested_file() {
    let result = parse_fs(
      r"
parent
  child
    filename #file
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "parent/child/filename".into(),
          vars: Default::default(),
        }],
        unique_vars: Default::default(),
      }
    );
  }

  #[test]
  fn parse_multiple_nested_subtrees() {
    let result = parse_fs(
      r"
parent
  child
    filename #file
parent_2
  child
    filename #file_2
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![
          FsPath {
            name: "file".into(),
            path_format: "parent/child/filename".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_2".into(),
            path_format: "parent_2/child/filename".into(),
            vars: Default::default(),
          },
        ],
        unique_vars: Default::default(),
      }
    );
  }

  // A complex tree must include:
  // - 2 files at the root, and in 2 nested subtrees at different levels;
  // - 2 directories at the root, and in 2 nested subtrees at different levels;
  // - 2 deeply nested files;
  #[test]
  fn parse_complex_tree() {
    let result = parse_fs(
      r"
filename_1 #file_1
filename_2 #file_2
parent_1
  filename_3 #file_3
  filename_4 #file_4
  child_1
    very
      deeply
        nested
          filename_5 #file_5
  child_2
    filename_6 #file_6
parent_2
  child_1
    very
      deeply
        nested
          filename_7 #file_7
          filename_8 #file_8
          child_1
            filename_9 #file_9
          child_2
            filename_10 #file_10
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![
          FsPath {
            name: "file_1".into(),
            path_format: "filename_1".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_2".into(),
            path_format: "filename_2".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_3".into(),
            path_format: "parent_1/filename_3".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_4".into(),
            path_format: "parent_1/filename_4".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_5".into(),
            path_format: "parent_1/child_1/very/deeply/nested/filename_5".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_6".into(),
            path_format: "parent_1/child_2/filename_6".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_7".into(),
            path_format: "parent_2/child_1/very/deeply/nested/filename_7".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_8".into(),
            path_format: "parent_2/child_1/very/deeply/nested/filename_8".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_9".into(),
            path_format: "parent_2/child_1/very/deeply/nested/child_1/filename_9".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_10".into(),
            path_format: "parent_2/child_1/very/deeply/nested/child_2/filename_10".into(),
            vars: Default::default(),
          },
        ],
        unique_vars: Default::default(),
      }
    );
  }

  #[test]
  fn var_parse_single_file() {
    let result = parse_fs("<Filename> #file");

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "{}".into(),
          vars: vec!["Filename".into()],
        }],
        unique_vars: HashSet::from(["Filename".into()]),
      }
    );
  }

  #[test]
  fn var_parse_nested_file() {
    let result = parse_fs(
      r"
<Parent>
  <Filename> #file
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "{}/{}".into(),
          vars: vec!["Parent".into(), "Filename".into()],
        }],
        unique_vars: HashSet::from(["Parent".into(), "Filename".into()]),
      }
    );
  }

  #[test]
  fn var_parse_multiple_nested_file() {
    let result = parse_fs(
      r"
<Parent>
  <Child>
    <Filename> #file
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![FsPath {
          name: "file".into(),
          path_format: "{}/{}/{}".into(),
          vars: vec!["Parent".into(), "Child".into(), "Filename".into()],
        }],
        unique_vars: HashSet::from(["Parent".into(), "Child".into(), "Filename".into()]),
      }
    );
  }

  #[test]
  fn var_parse_multiple_nested_subtrees() {
    let result = parse_fs(
      r"
<Parent>
  <Child>
    <Filename> #file
<Parent2>
  <Child>
    <Filename2> #file_2
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![
          FsPath {
            name: "file".into(),
            path_format: "{}/{}/{}".into(),
            vars: vec!["Parent".into(), "Child".into(), "Filename".into()],
          },
          FsPath {
            name: "file_2".into(),
            path_format: "{}/{}/{}".into(),
            vars: vec!["Parent2".into(), "Child".into(), "Filename2".into()],
          },
        ],
        unique_vars: HashSet::from([
          "Parent".into(),
          "Child".into(),
          "Filename".into(),
          "Parent2".into(),
          "Filename2".into(),
        ]),
      }
    );
  }

  // A complex tree with variables must:
  // - include 2 files at the root, and in 2 nested subtrees at different levels;
  // - include 2 directories at the root, and in 2 nested subtrees at different
  //   levels;
  // - include 2 deeply nested files;
  // - make every other nested entry variable;
  // - include an entry that is entirely variable;
  // - include an entry that is variable at the start;
  // - include an entry that is variable in the middle;
  // - include an entry that is variable at the end;
  // - include a directory name with 3 variables;
  // - include a filename with 3 variables and a variable extension;
  // - include a variable filename with a hardcoded extension;
  #[test]
  fn var_parse_complex_tree() {
    let result = parse_fs(
      r"
<Filename1>_file #file_1
file_<Filename2>_file #file_2
parent_1
  filename_3 #file_3
  <VarName1>_<VarName2>_<VarName3>.<Extension1> #file_4
  child_<DirectoryName1>
    very
      <DirectoryName2>
        nested
          <Filename3>.txt #file_5
  child_<DirectoryName3>
    filename_6 #file_6
<DirectoryName4>
  child_1
    <VarName4>_<VarName5>_<VarName6>
      deeply
        <Nested>
          filename_7 #file_7
          filename_8 #file_8
          child_1
            filename.<Extension2> #file_9
          child_2
            filename_10 #file_10
",
    );

    assert_eq!(
      result,
      ParsedFs {
        paths: vec![
          FsPath {
            name: "file_1".into(),
            path_format: "{}_file".into(),
            vars: vec!["Filename1".into()],
          },
          FsPath {
            name: "file_2".into(),
            path_format: "file_{}_file".into(),
            vars: vec!["Filename2".into()],
          },
          FsPath {
            name: "file_3".into(),
            path_format: "parent_1/filename_3".into(),
            vars: Default::default(),
          },
          FsPath {
            name: "file_4".into(),
            path_format: "parent_1/{}_{}_{}.{}".into(),
            vars: vec![
              "VarName1".into(),
              "VarName2".into(),
              "VarName3".into(),
              "Extension1".into(),
            ],
          },
          FsPath {
            name: "file_5".into(),
            path_format: "parent_1/child_{}/very/{}/nested/{}.txt".into(),
            vars: vec![
              "DirectoryName1".into(),
              "DirectoryName2".into(),
              "Filename3".into(),
            ],
          },
          FsPath {
            name: "file_6".into(),
            path_format: "parent_1/child_{}/filename_6".into(),
            vars: vec!["DirectoryName3".into()],
          },
          FsPath {
            name: "file_7".into(),
            path_format: "{}/child_1/{}_{}_{}/deeply/{}/filename_7".into(),
            vars: vec![
              "DirectoryName4".into(),
              "VarName4".into(),
              "VarName5".into(),
              "VarName6".into(),
              "Nested".into(),
            ],
          },
          FsPath {
            name: "file_8".into(),
            path_format: "{}/child_1/{}_{}_{}/deeply/{}/filename_8".into(),
            vars: vec![
              "DirectoryName4".into(),
              "VarName4".into(),
              "VarName5".into(),
              "VarName6".into(),
              "Nested".into(),
            ],
          },
          FsPath {
            name: "file_9".into(),
            path_format: "{}/child_1/{}_{}_{}/deeply/{}/child_1/filename.{}".into(),
            vars: vec![
              "DirectoryName4".into(),
              "VarName4".into(),
              "VarName5".into(),
              "VarName6".into(),
              "Nested".into(),
              "Extension2".into(),
            ],
          },
          FsPath {
            name: "file_10".into(),
            path_format: "{}/child_1/{}_{}_{}/deeply/{}/child_2/filename_10".into(),
            vars: vec![
              "DirectoryName4".into(),
              "VarName4".into(),
              "VarName5".into(),
              "VarName6".into(),
              "Nested".into(),
            ],
          },
        ],
        unique_vars: HashSet::from([
          "Filename1".into(),
          "Filename2".into(),
          "Filename3".into(),
          "DirectoryName1".into(),
          "DirectoryName2".into(),
          "DirectoryName3".into(),
          "DirectoryName4".into(),
          "VarName1".into(),
          "VarName2".into(),
          "VarName3".into(),
          "VarName4".into(),
          "VarName5".into(),
          "VarName6".into(),
          "Extension1".into(),
          "Extension2".into(),
          "Nested".into(),
        ]),
      }
    );
  }
}
