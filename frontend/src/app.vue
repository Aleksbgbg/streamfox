<script setup lang="ts">
import CToastArea from "@/components/toasts/toast-area.vue";
import CNavbar from "@/views/navbar/navbar.vue";
</script>

<template lang="pug">
#app.flex.flex-col.bg-polar-dark.text-white.overflow-hidden.h-screen
  header.bg-polar-darkest
    c-navbar
  main.grow.overflow-auto
    router-view
  c-toast-area
</template>

<style lang="stylus">
#app
  font-family: "Open Sans", sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
</style>
