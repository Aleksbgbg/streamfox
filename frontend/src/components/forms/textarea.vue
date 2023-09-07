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
</script>

<template lang="pug">
label.justify-self-end.mt-2(:for="label")
  span {{ title }}
  span.text-aurora-red *
.flex.flex-col
  textarea(
    class="bg-polar-darkest text-white placeholder-snow-dark placeholder:italic focus:border-aurora-green focus:outline-none resize-none leading-normal rounded border py-2 px-3"
    :class="[(errors && errors.length > 0) ? 'border-aurora-red' : 'border-polar-lightest']"
    :id="label" :label="label" :placeholder="placeholder"
    :value="modelValue"
    @input="$emit('update:modelValue', asInputElement($event.target).value)"
  )
  p.text-aurora-red(v-for="error of errors") {{ error }}
</template>
