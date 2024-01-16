import { check } from "@/utils/null";

export class Option<T> {
  private readonly _value: T | null;

  private constructor(value: T | null) {
    this._value = value;
  }

  public static some<T>(value: NonNullable<T>): Option<NonNullable<T>> {
    return new Option(value);
  }

  public static none<T>(): Option<NonNullable<T>> {
    return new Option<NonNullable<T>>(null);
  }

  public get(): T {
    return check(this._value);
  }

  public isSome(): boolean {
    return !this.isNone();
  }

  public isNone(): boolean {
    return this._value === null;
  }

  public isSomeAnd(map: (val: T) => boolean): boolean {
    return this.isSome() && map(this.get());
  }

  public mapOrElse<R>(map: (val: T) => R, fallback: R): R {
    return this.isSome() ? map(this.get()) : fallback;
  }
}

export const some = Option.some;
export const none = Option.none;
