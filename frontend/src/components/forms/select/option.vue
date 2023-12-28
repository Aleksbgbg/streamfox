<script setup lang="ts">
import { onMounted, onUnmounted, toRefs } from "vue";
import CDropdownItem from "@/components/dropdown/item.vue";
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
c-dropdown-item(@click="select") {{ title }}
</template>
