import { localGetBool, localSet } from "@/utils/local-storage";

const LOOP_KEY = "loop";

export function getLoop(): boolean {
  return localGetBool(LOOP_KEY, false);
}

export function setLoop(value: boolean) {
  localSet(LOOP_KEY, value.toString());
}
