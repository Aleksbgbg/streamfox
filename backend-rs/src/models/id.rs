use sea_orm::{DbErr, DeriveValueType, TryFromU64};

#[derive(Clone, Debug, PartialEq, Eq, DeriveValueType)]
pub struct Id(i64);

impl Id {
  pub fn from(value: i64) -> Self {
    Self(value)
  }
}

impl TryFromU64 for Id {
  fn try_from_u64(n: u64) -> Result<Self, DbErr> {
    Ok(Id(n.try_into().map_err(
      |err: <i64 as TryFrom<u64>>::Error| DbErr::TryIntoErr {
        from: stringify!(u64),
        into: stringify!(i64),
        source: err.into(),
      },
    )?))
  }
}

impl ToString for Id {
  fn to_string(&self) -> String {
    let mut string = String::new();
    bs58::encode(self.0.to_be_bytes())
      .onto(&mut string)
      .expect("could not encode ID into base 58");
    string
  }
}
