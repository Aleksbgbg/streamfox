import { panic } from "@/utils/panic";

export type Optional<T> = T | null;

export function empty<T>(): Optional<T> {
  return null;
}

export function hasValue<T>(opt: Optional<T>): boolean {
  return opt !== null;
}

export function getValue<T>(opt: Optional<T>): T {
  return opt ?? panic("no value was available in optional");
}

export function mapOrElse<T, O>(opt: Optional<T>, map: (val: T) => O, fallback: O): O {
  if (hasValue(opt)) {
    return map(getValue(opt));
  } else {
    return fallback;
  }
}

export function tryApply<T>(opt: Optional<T>, func: (val: T) => void) {
  if (hasValue(opt)) {
    func(getValue(opt));
  }
}
