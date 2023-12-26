import { panic } from "@/utils/panic";

export function getStrict<TKey, TValue>(map: Map<TKey, TValue>, key: TKey): TValue {
  return map.get(key) ?? panic(`could not find key '${key}' in map`);
}
