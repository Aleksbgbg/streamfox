import { check } from "@/utils/null";

export function getStrict<TKey, TValue>(map: Map<TKey, TValue>, key: TKey): TValue {
  return check(map.get(key), "could not find key in map");
}
