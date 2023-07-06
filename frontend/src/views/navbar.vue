<script setup lang="ts">
import CUploadButton from "@/components/upload-button.vue";
</script>

<template lang="pug">
nav.flex.h-20.p-5
  router-link(:to="{ name: 'home' }")
    h1.font-bold.text-2xl Streamfox
  c-upload-button.flex-grow
  .self-center
    router-link.rounded.hover_bg-polar-light.px-3.py-2(:to="{ name: 'login' }")
      i.bi-box-arrow-in-right
      span Log In
</template>
