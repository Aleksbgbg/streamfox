import { panic } from "@/utils/panic";

export function asInputElement(eventTarget: EventTarget | null) {
  return (eventTarget ?? panic("no event target")) as HTMLInputElement;
}
