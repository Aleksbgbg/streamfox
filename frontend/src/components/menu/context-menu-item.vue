<script setup lang="ts">
import CIcon from "@/components/icon.vue";

defineProps<{
  icon: string;
}>();
</script>

<template lang="pug">
// Button clicks get supressed by menu hiding,
// anchor tags do not suffer from this
a.flex.items-center.px-3.py-1(
  class="hover:bg-frost-blue"
  role="button"
)
  c-icon.text-2xl.mr-2(:name="icon")
  span.text-sm
    slot
</template>
