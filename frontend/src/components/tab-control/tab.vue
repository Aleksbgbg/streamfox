<script setup lang="ts">
import { onMounted, onUnmounted, ref, toRef } from "vue";
import {
  type TabControl,
  type TabKey,
  type TabRef,
  tabControlKey,
} from "@/components/tab-control/tab-control";
import { injectStrict } from "@/utils/inject";
import { check } from "@/utils/null";

const emit = defineEmits<{
  (e: "selected"): void;
}>();

const props = defineProps<{
  title: string;
}>();

const tabControl = injectStrict<TabControl>(tabControlKey);

const self: TabRef = ref(null);

const isActive = ref(false);

let key: TabKey;
onMounted(() => {
  key = tabControl.attach({
    ref: check(self.value),
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
  tabControl.detach(key);
});
</script>

<template lang="pug">
section.grow.w-full.min-h-0.max-h-full(ref="self" v-show="isActive")
  slot
</template>
