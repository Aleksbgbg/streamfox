import type { InjectionKey, Ref } from "vue";

export const tabControlKey = Symbol() as InjectionKey<TabControl>;

export type TabRef = Ref<HTMLElement | null>;

export type TabKey = symbol;
export function newKey(): TabKey {
  return Symbol();
}
export interface TabControl {
  attach(child: TabChild): TabKey;
  detach(key: TabKey): void;
}

export interface TabChild {
  ref: HTMLElement;
  title: Ref<string>;

  activate(): void;
  deactivate(): void;
}
