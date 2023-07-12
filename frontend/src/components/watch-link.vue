<script setup lang="ts">
defineProps<{
  videoId: string;
}>();
</script>

<template lang="pug">
router-link(:to="{ name: 'watch', params: { id: videoId } }")
  slot
</template>
