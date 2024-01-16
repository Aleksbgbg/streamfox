<script setup lang="ts">
import { provide, reactive, ref } from "vue";
import {
  type TabChild,
  type TabControl,
  tabControlKey,
} from "@/components/tab-control/tab-control";

interface Props {
  vertical?: boolean;
}
withDefaults(defineProps<Props>(), {
  vertical: false,
});

const children: TabChild[] = reactive([]);
const activeIndex = ref(-1);

function switchActiveTab(newIndex: number) {
  const lastIndex = activeIndex.value;
  if (lastIndex !== -1) {
    children[lastIndex].deactivate();
  }

  activate(newIndex);
}

function activate(newIndex: number) {
  activeIndex.value = newIndex;
  if (newIndex !== -1) {
    children[newIndex].activate();
  }
}

provide<TabControl>(tabControlKey, {
  attach(child) {
    const newIndex = children.length;

    children.push(child);

    if (newIndex === 0) {
      switchActiveTab(0);
    }

    return newIndex;
  },
  detach(index) {
    children.splice(index, 1);

    if (children.length > 0) {
      if (activeIndex.value >= index) {
        activate(activeIndex.value - 1);
      }
    } else {
      activeIndex.value = -1;
    }
  },
});
</script>

<template lang="pug">
.flex-grow.flex.gap-5.w-full.min-h-0(:class="{ 'flex-col': !vertical }")
  div(:class="vertical ? 'flex flex-col min-w-0 w-1/3 overflow-y-auto' : 'grid grid-cols-[repeat(auto-fit,_minmax(0,_1fr))]'")
    .flex(
      class="hover:bg-polar-lightest hover:cursor-pointer"
      :class="{ 'flex-col': !vertical, 'flex-row-reverse w-full': vertical }"
      v-for="(child, index) of children"
      :key="index"
    )
      button.flex-grow.text-center.truncate.py-1.px-2(
        type="button"
        @click="switchActiveTab(index)"
      ) {{ child.title || '(unnamed)' }}
      .shrink-0(
        :class="{ 'bg-frost-blue': index === activeIndex, 'w-1': vertical, 'h-1': !vertical }"
      )
  slot
</template>
