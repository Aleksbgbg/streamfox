mod parse_fs;

use proc_macro2::TokenStream;
use quote::{format_ident, quote};
use std::str::FromStr;
use syn::parse::{Parse, ParseStream};
use syn::{parse_macro_input, Ident, LitStr, Token};

struct Filesystem {
  name: Ident,
  tree: LitStr,
}

impl Parse for Filesystem {
  fn parse(input: ParseStream) -> syn::Result<Self> {
    let name: Ident = input.parse()?;
    input.parse::<Token![,]>()?;
    let tree: LitStr = input.parse()?;

    Ok(Filesystem { name, tree })
  }
}

fn as_tokens(string: String) -> TokenStream {
  TokenStream::from_str(&string).unwrap()
}

#[proc_macro]
pub fn filesystem(input: proc_macro::TokenStream) -> proc_macro::TokenStream {
  let Filesystem { name, tree } = parse_macro_input!(input as Filesystem);

  let tree = parse_fs::parse_fs(&tree.value());

  let vars_enum = format_ident!("{}Var", name);
  let unique_vars = tree.unique_vars.into_iter().map(as_tokens);
  let funcs = tree.paths.into_iter().map(|path| {
    let name = as_tokens(path.name);
    let path_format = path.path_format;
    let vars: TokenStream = path
      .vars
      .into_iter()
      .map(as_tokens)
      .map(|var| quote!(self.vars.get(&#vars_enum::#var).unwrap(),))
      .collect();

    quote! {
      pub fn #name(&self) -> ::fs_file::File {
        ::fs_file::File::from(format!(#path_format, #vars))
      }
    }
  });

  proc_macro::TokenStream::from(quote! {
    #[derive(
      ::std::cmp::PartialEq,
      ::std::cmp::Eq,
      ::std::hash::Hash,
      ::std::clone::Clone,
      ::std::marker::Copy,
    )]
    pub enum #vars_enum {
      #(#unique_vars),*
    }

    #[derive(::std::clone::Clone, ::std::default::Default)]
    pub struct #name {
      vars: ::std::collections::HashMap<#vars_enum, String>,
    }

    impl #name {
      pub fn new() -> Self {
        Default::default()
      }

      pub fn set(&mut self, var: #vars_enum, val: String) -> &mut Self {
        self.vars.insert(var, val);
        self
      }

      #(#funcs)*
    }
  })
}
