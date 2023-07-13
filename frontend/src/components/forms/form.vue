<script setup lang="ts">
defineEmits<{
  (e: "submit"): void;
}>();

defineProps<{
  title: string;
}>();
</script>

<template lang="pug">
.bg-polar-darkest.border.border-frost-deep.rounded-t.overflow-hidden.m-5(class="w-2/3")
  h2.bg-polar-dark.border-b.border-frost-deep.text-center.py-2 {{ title }}
  form.flex.flex-col.items-center.my-4(@submit.prevent="$emit('submit')")
    slot
</template>
