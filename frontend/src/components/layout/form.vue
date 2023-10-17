<script setup lang="ts">
defineProps<{
  title: string;
}>();
</script>

<template lang="pug">
.flex.justify-center.mt-5
  .bg-polar-darkest.border.border-frost-deep.rounded-t.overflow-hidden(class="w-2/3")
    h2.bg-polar-dark.border-b.border-frost-deep.text-center.py-2 {{ title }}
    slot
</template>
