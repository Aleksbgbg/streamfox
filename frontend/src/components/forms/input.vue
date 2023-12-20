<script setup lang="ts">
import { computed, ref } from "vue";
import CButton from "@/components/button.vue";
import CErrors from "@/components/forms/errors.vue";
import CIcon from "@/components/icon.vue";
import { asInputElement } from "@/utils/cast";
import { toLowerCamelCase } from "@/utils/strings";

defineEmits<{
  (e: "update:modelValue", value: string): void;
}>();

interface Props {
  center?: boolean;
  title: string;
  type?: "text" | "password";
  modelValue: string;
  errors?: string[];
}
const props = withDefaults(defineProps<Props>(), {
  center: false,
  type: "text",
});

const visible = ref(false);

const label = computed(() => toLowerCamelCase(props.title));
const valid = computed(() => !props.errors || props.errors.length === 0);
const inputType = computed(() =>
  props.type === "password" && visible.value ? "text" : props.type,
);
</script>

<template lang="pug">
div(:class="center ? 'w-80 max-w-full px-5' : 'w-full'")
  .group.relative.rounded.ring-1.ring-frost-blue.ring-inset.w-full.py-2.px-3(
    class="hover:ring-2 focus-within:ring-2 focus-within:ring-aurora-yellow aria-invalid:[&:not(:focus)]:ring-aurora-red"
    :aria-invalid="!valid"
  )
    label.absolute.pointer-events-none.bg-polar-light.truncate.max-w-full.px-1.-ml-1.transition-all.duration-100.ease-linear(
      class="group-focus-within:text-white group-focus-within:text-xs group-focus-within:-mt-4"
      :class="modelValue.length > 0 ? 'text-white text-xs -mt-4' : 'text-snow-dark'"
      :for="label"
    ) {{ title }}
    .flex
      input.grow.bg-transparent(
        class="focus:outline-none"
        :id="label"
        :label="label"
        :type="inputType"
        :value="modelValue"
        @input="$emit('update:modelValue', asInputElement($event.target).value)"
      )
      c-button.ml-2(
        v-if="type === 'password'"
        class="focus:ring-1 ring-frost-green focus:outline-none"
        type="button"
        theme="invisible"
        padding="slim"
        @mousedown="(e) => e.preventDefault()"
        @click="visible = !visible"
      )
        c-icon(:name="visible ? 'eye-slash-fill' : 'eye-fill'")
  c-errors(:errors="errors")
</template>
