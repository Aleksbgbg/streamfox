import type { InjectionKey, Ref } from "vue";

export const tabControlKey = Symbol() as InjectionKey<TabControl>;

export type TabRef = Ref<HTMLElement | null>;

export interface TabControl {
  attach(child: TabChild): number;
  detach(index: number): void;
}

export interface TabChild {
  ref: HTMLElement;
  title: Ref<string>;

  activate(): void;
  deactivate(): void;
}
