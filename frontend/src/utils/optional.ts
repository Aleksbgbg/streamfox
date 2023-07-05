export type Optional<T> = T | null;

export function empty<T>(): Optional<T> {
  return null;
}

export function hasValue<T>(opt: Optional<T>): boolean {
  return opt !== null;
}

export function getValue<T>(opt: Optional<T>): T {
  return opt as T;
}
