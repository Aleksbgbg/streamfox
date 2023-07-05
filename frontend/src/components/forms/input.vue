<script setup lang="ts">
import { computed } from "vue";
import { toLowerCamelCase } from "@/utils/strings";

defineEmits<{
  (e: "update:modelValue", value: string): void;
}>();

interface Props {
  title: string;
  placeholder?: string;
  type?: string;
  modelValue: string;
  errors: string[];
}

const props = withDefaults(defineProps<Props>(), {
  type: "text",
});

const label = computed(() => toLowerCamelCase(props.title));
</script>

<template lang="pug">
label.justify-self-end.mt-2(:for="label")
  span {{ title }}
  span.text-aurora-red *
.flex.flex-col
  input.bg-polar-darkest.rounded.border.focus_border-aurora-green.focus_outline-none.leading-normal.text-white.placeholder-white.py-2.px-3(
    :class="[errors.length === 0 ? 'border-polar-lightest' : 'border-aurora-red']"
    :id="label" :label="label" :placeholder="placeholder" :type="type"
    :value="modelValue" @input="$emit('update:modelValue', $event.target.value)"
  )
  p.text-aurora-red(v-for="error of errors") {{ error }}
</template>
