<script setup lang="ts">
import { onMounted, onUnmounted, toRefs } from "vue";
import { type SelectControl, selectControlKey } from "@/components/forms/select/select-control";
import { injectStrict } from "@/utils/inject";

const props = defineProps<{
  title: string;
  value: number;
}>();

const self = toRefs(props);

const selectControl = injectStrict<SelectControl>(selectControlKey);

function select() {
  selectControl.select(self);
}

onMounted(() => {
  selectControl.add(self);
});
onUnmounted(() => {
  selectControl.remove(self);
});
</script>

<template lang="pug">
li.bg-polar-darkest.truncate.max-w-full.px-3.py-2(
  class="hover:bg-polar-dark"
  @click="select"
) {{ title }}
</template>
