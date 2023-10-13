<script setup lang="ts">
import { onMounted, onUnmounted, ref, toRef } from "vue";
import { type TabControl, type TabRef, tabControlKey } from "@/components/tab-control/tab-control";
import { injectStrict } from "@/utils/inject";
import { panic } from "@/utils/panic";

const emit = defineEmits<{
  (e: "selected"): void;
}>();

const props = defineProps<{
  title: string;
}>();

const tabControl = injectStrict<TabControl>(tabControlKey);

const self: TabRef = ref(null);

const isActive = ref(false);

let index = -1;
onMounted(() => {
  index = tabControl.attach({
    ref: self.value ?? panic("self not available"),
    title: toRef(props, "title"),
    activate() {
      emit("selected");
      isActive.value = true;
    },
    deactivate() {
      isActive.value = false;
    },
  });
});
onUnmounted(() => {
  tabControl.detach(index);
});
</script>

<template lang="pug">
section.grow.w-full.min-h-0.max-h-full(ref="self" v-show="isActive")
  slot
</template>
