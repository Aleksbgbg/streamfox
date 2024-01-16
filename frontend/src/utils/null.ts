import { panic } from "@/utils/panic";

export function check<T>(value: T): NonNullable<T> {
  return value ?? panic("checked value is null or undefined");
}
