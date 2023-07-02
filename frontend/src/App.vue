<script setup lang="ts">
import VNavbar from "@/views/navbar.vue";
</script>

<template lang="pug">
#app.grid.grid-rows-6.bg-polar-dark.text-white.h-screen
  header.row-end-1.bg-theme-darkest.shadow-2xl
    v-navbar
  main.row-start-1.row-end-7.overflow-auto
    router-view
</template>

<style lang="stylus">
#app
  font-family: Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
</style>
