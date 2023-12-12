import { type InjectionKey, inject } from "vue";
import { panic } from "@/utils/panic";

export function injectStrict<T>(key: InjectionKey<T>): T {
  const result = inject(key);

  if (result === undefined) {
    panic(`no provider for injection key ${key.toString()}`);
  }

  return result as T;
}
