export function panic(message: string): never {
  throw panicErr(message);
}

export function panicErr(message: string) {
  return new Error(message);
}
