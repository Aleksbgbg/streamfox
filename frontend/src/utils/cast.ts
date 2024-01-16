import { check } from "@/utils/null";

export function asInputElement(eventTarget: EventTarget | null) {
  return check(eventTarget) as HTMLInputElement;
}
