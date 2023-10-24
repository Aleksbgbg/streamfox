<script setup lang="ts">
defineProps<{
  title: string;
}>();
</script>

<template lang="pug">
.flex.h-full.max-h-full(class="md:justify-center md:h-auto md:py-5")
  .flex.flex-col.bg-polar-light.border-frost-deep.w-full.max-h-full.overflow-hidden(
    class="md:border md:rounded-t md:w-4/5"
  )
    h2.bg-polar-dark.border-b.border-frost-deep.text-center.py-2 {{ title }}
    slot
</template>
