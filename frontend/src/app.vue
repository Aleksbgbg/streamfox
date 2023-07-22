<script setup lang="ts">
import VNavbar from "@/views/navbar.vue";
</script>

<template lang="pug">
#app.flex.flex-col.bg-polar-dark.text-white.overflow-hidden.h-screen
  header.bg-polar-darkest
    v-navbar
  main.grow.overflow-auto
    router-view
</template>

<style lang="stylus">
#app
  font-family: "Open Sans", sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
</style>
