<script setup lang="ts">
import { onMounted, provide, reactive, shallowRef } from "vue";
import {
  type TabChild,
  type TabControl,
  type TabKey,
  newKey,
  tabControlKey,
} from "@/components/tab-control/tab-control";
import { type Option, none, some } from "@/types/option";
import { getStrict } from "@/utils/maps";

interface Props {
  vertical?: boolean;
}
withDefaults(defineProps<Props>(), {
  vertical: false,
});

const children = reactive(new Map<TabKey, TabChild>());
const active = shallowRef(none<TabKey>());

function activate(key: Option<TabKey>) {
  if (key.eq(active.value)) {
    return;
  }

  active.value.ifSome((value) => getStrict(children, value).deactivate());
  key.ifSome((value) => getStrict(children, value).activate());

  active.value = key;
}

let focusLatestChild = false;
onMounted(() => {
  focusLatestChild = true;
});

provide<TabControl>(tabControlKey, {
  attach(child) {
    const id = newKey();

    children.set(id, reactive(child));
    if (focusLatestChild || children.size === 1) {
      activate(some(id));
    }

    return id;
  },
  detach(id) {
    if (id === active.value.get()) {
      let newActive = none<TabKey>();

      for (const key of children.keys()) {
        if (key !== id) {
          newActive = some(key);
          break;
        }
      }

      activate(newActive);
    }

    children.delete(id);
  },
});
</script>

<template lang="pug">
.flex.gap-5.w-full.h-full.min-h-0(:class="{ 'flex-col': !vertical }")
  div(:class="vertical ? 'flex flex-col min-w-0 w-1/3 overflow-y-auto' : 'grid grid-cols-[repeat(auto-fit,_minmax(0,_1fr))]'")
    .flex(
      class="hover:bg-polar-lightest hover:cursor-pointer"
      :class="{ 'flex-col': !vertical, 'flex-row-reverse w-full': vertical }"
      v-for="[key, child] of children"
      :key="key"
    )
      button.flex-grow.text-center.truncate.py-1.px-2(
        type="button"
        @click="activate(some(key))"
      ) {{ child.title || '(unnamed)' }}
      .shrink-0(
        :class="{ 'bg-frost-blue': active.get() === key, 'w-1': vertical, 'h-1': !vertical }"
      )
  slot
</template>
