<script setup lang="ts">
defineProps<{
  toggled: boolean;
}>();
</script>

<template lang="pug">
span.text-sm.transition-all.duration-300(:class="{ 'rotate-180': toggled }") ▼
</template>
