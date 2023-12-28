<script setup lang="ts">
import { type Ref, computed, provide, ref, watch } from "vue";
import CDropdownArrow from "@/components/dropdown/arrow.vue";
import CDropdown from "@/components/dropdown/dropdown.vue";
import CErrors from "@/components/forms/errors.vue";
import {
  type SelectChild,
  type SelectControl,
  selectControlKey,
} from "@/components/forms/select/select-control";
import { toLowerCamelCase } from "@/utils/strings";

const emit = defineEmits<{
  (e: "update:modelValue", value: number): void;
}>();

interface Props {
  center?: boolean;
  title: string;
  modelValue: number;
  errors?: string[];
}
const props = withDefaults(defineProps<Props>(), {
  center: false,
});

const label = computed(() => toLowerCamelCase(props.title));
const valid = computed(() => !props.errors || props.errors.length === 0);
const hasSelected = computed(() => selected.value !== null);

const selected: Ref<SelectChild | null> = ref(null);

function select(child: SelectChild) {
  selected.value = child;
  emit("update:modelValue", child.value.value);
}

const children = new Map();

watch(
  () => props.modelValue,
  (newValue) => {
    if (children.has(newValue)) {
      select(children.get(newValue));
    }
  },
);

provide<SelectControl>(selectControlKey, {
  add(child) {
    children.set(child.value.value, child);

    if (props.modelValue === child.value.value) {
      this.select(child);
    }
  },
  remove(child) {
    children.delete(child.value.value);
  },
  select,
});
</script>

<template lang="pug">
div(:class="center ? 'w-80 max-w-full px-5' : 'w-full'")
  label.absolute.pointer-events-none.bg-polar-light.truncate.max-w-full.px-1.ml-2.z-10.transition-all.duration-100.ease-linear(
    :class="hasSelected ? 'text-white text-xs -mt-2' : 'text-snow-dark mt-2'"
    :for="label"
  ) {{ title }}
  c-dropdown
    slot
    template(#button="{ toggled }")
      button.flex.bg-transparent.rounded.ring-1.ring-frost-blue.ring-inset.w-full.h-10.py-2.px-3(
        class="aria-invalid:[&:not(:focus)]:ring-aurora-red hover:ring-2 focus:ring-2 focus:ring-aurora-yellow focus:outline-none"
        type="button"
        :id="label"
        :aria-invalid="!valid"
      )
        span.grow.text-left.truncate {{ selected?.title }}
        c-dropdown-arrow.self-center.ml-2(:toggled="toggled")
  c-errors(:errors="errors")
</template>
