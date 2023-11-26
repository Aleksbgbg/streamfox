<script setup lang="ts">
defineProps<{
  errors?: string[];
}>();
</script>

<template lang="pug">
div(v-show="errors && errors.length > 0")
  p.text-aurora-red-400(v-for="error of errors") {{ error }}
</template>
