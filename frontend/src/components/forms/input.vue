<script setup lang="ts">
import { computed } from "vue";
import { asInputElement } from "@/utils/cast";
import { toLowerCamelCase } from "@/utils/strings";

defineEmits<{
  (e: "update:modelValue", value: string): void;
}>();

interface Props {
  center?: boolean;
  title: string;
  type?: string;
  modelValue: string;
  errors?: string[];
}
const props = withDefaults(defineProps<Props>(), {
  center: false,
  type: "text",
});

const label = computed(() => toLowerCamelCase(props.title));
const valid = computed(() => !props.errors || props.errors.length === 0);
</script>

<template lang="pug">
div(:class="center ? 'w-80 max-w-full px-5' : 'w-full'")
  .group.relative
    label.absolute.pointer-events-none.bg-polar-light.truncate.max-w-full.px-1.ml-2.transition-all.duration-100.ease-linear(
      class="group-focus-within:text-white group-focus-within:text-xs group-focus-within:-mt-2"
      :class="modelValue.length > 0 ? 'text-white text-xs -mt-2' : 'text-snow-dark mt-2'"
      :for="label"
    ) {{ title }}
    input.bg-transparent.rounded.ring-1.ring-frost-blue.ring-inset.w-full.py-2.px-3(
      class="aria-invalid:[&:not(:focus)]:ring-aurora-red hover:ring-2 focus:ring-2 focus:ring-aurora-yellow focus:outline-none"
      :id="label"
      :label="label"
      :type="type"
      :value="modelValue"
      :aria-invalid="!valid"
      @input="$emit('update:modelValue', asInputElement($event.target).value)"
    )
  p.text-aurora-red(v-for="error of errors") {{ error }}
</template>
