import type { InjectionKey, Ref } from "vue";

export const selectControlKey = Symbol() as InjectionKey<SelectControl>;

export interface SelectControl {
  add(child: SelectChild): void;
  remove(child: SelectChild): void;

  select(child: SelectChild): void;
}

export interface SelectChild {
  value: Ref<number>;
  title: Ref<string>;
}
