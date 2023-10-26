<script setup lang="ts">
import { computed } from "vue";
import { asInputElement } from "@/utils/cast";
import { toLowerCamelCase } from "@/utils/strings";

defineEmits<{
  (e: "update:modelValue", value: string): void;
}>();

interface Props {
  title: string;
  placeholder?: string;
  modelValue: string;
  errors?: string[];
}
const props = defineProps<Props>();

const label = computed(() => toLowerCamelCase(props.title));
const valid = computed(() => !props.errors || props.errors.length === 0);
</script>

<template lang="pug">
.flex.flex-col.w-full.h-full
  .group.flex-grow.flex.flex-col.rounded.ring-1.ring-frost-blue.ring-inset(
    class="data-invalid:[&:not(:focus-within)]:ring-aurora-red hover:ring-2 focus-within:ring-2 focus-within:ring-aurora-yellow"
    :data-invalid="!valid"
  )
    label.truncate.max-w-full.pl-3.pt-2(
      class="group-focus-within:text-aurora-yellow-300"
      :for="label"
    ) {{ title }}
    textarea.flex-grow.bg-transparent.resize-none.pl-3.py-2.mr-1.mb-1(
      class="focus:outline-none"
      :id="label"
      :label="label"
      :placeholder="placeholder"
      :value="modelValue"
      :aria-invalid="!valid"
      @input="$emit('update:modelValue', asInputElement($event.target).value)"
    )
  p.text-aurora-red(v-for="error of errors") {{ error }}
</template>
