<script setup lang="ts">
import CBackgroundImage from "@/assets/low-poly-grid-haikei.svg";
import CToastArea from "@/components/toasts/toast-area.vue";
import CNavbar from "@/views/navbar/navbar.vue";
</script>

<template lang="pug">
#app.flex.flex-col.bg-polar-dark.text-white.overflow-hidden.h-screen
  header.bg-polar-darkest(:style="{ 'z-index': 1 }")
    c-navbar
  main.grow.overflow-auto
    c-background-image.fixed.inset-0(:style="{ 'z-index': 0 }")
    router-view
  c-toast-area
</template>

<style lang="stylus">
#app
  font-family: "Open Sans", sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
</style>
