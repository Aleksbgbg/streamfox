<script setup lang="ts">
import { computed } from "vue";
import { asInputElement } from "@/utils/cast";
import { toLowerCamelCase } from "@/utils/strings";

defineEmits<{
  (e: "update:modelValue", value: unknown): void;
}>();

interface Props {
  title: string;
  modelValue: unknown;
  errors: string[];
}

const props = defineProps<Props>();

const label = computed(() => toLowerCamelCase(props.title));
</script>

<template lang="pug">
label.justify-self-end.mt-2(:for="label")
  span {{ title }}
  span.text-aurora-red *
.flex.flex-col
  select(
    class="bg-polar-darkest text-white placeholder-white focus:border-aurora-green focus:outline-none leading-normal rounded border py-2 px-3"
    :class="[errors.length === 0 ? 'border-polar-lightest' : 'border-aurora-red']"
    :id="label" :label="label"
    :value="modelValue"
    @input="$emit('update:modelValue', asInputElement($event.target).value)"
  )
    slot
  p.text-aurora-red(v-for="error of errors") {{ error }}
</template>
