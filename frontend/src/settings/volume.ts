import { localGetFloat, localSet } from "@/utils/local-storage";

const VOLUME_KEY = "volume";

export function getVolume(): number {
  return localGetFloat(VOLUME_KEY, 0.5);
}

export function setVolume(value: number) {
  localSet(VOLUME_KEY, value.toString());
}
