import { panic } from "@/utils/panic";

export function check<T>(value: T, message = "checked value is null or undefined"): NonNullable<T> {
  return value ?? panic(message);
}
