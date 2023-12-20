<script setup lang="ts">
defineProps<{
  name: string;
}>();
</script>

<template lang="pug">
i.bi(:class="`bi-${name}`")
</template>
