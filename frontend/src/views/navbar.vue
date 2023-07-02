<script setup lang="ts">
import CUploadButton from "@/components/upload-button.vue";
</script>

<template lang="pug">
nav.flex.h-20.p-5
  router-link(:to="{ name: 'home' }")
    h1.font-bold.text-2xl Streamfox
  c-upload-button
</template>
