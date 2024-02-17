use ring::error::Unspecified;
use ring::rand::{SecureRandom, SystemRandom};

pub struct Bytes(pub usize);

pub fn random_bytes(random: &SystemRandom, Bytes(len): Bytes) -> Result<Vec<u8>, Unspecified> {
  let mut bytes = vec![0; len];
  random.fill(&mut bytes)?;
  Ok(bytes)
}
