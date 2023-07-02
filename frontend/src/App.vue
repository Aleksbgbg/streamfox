<template lang="pug">
#app.grid.grid-rows-6.bg-polar-dark.text-white.h-screen
  header.row-end-1.bg-theme-darkest.h-20.p-5.shadow-2xl
    nav.flex
      router-link(:to="{ name: 'home' }")
        h1.font-bold.text-2xl Streamfox
      c-upload-button
  main.row-start-1.row-end-7.overflow-auto
    router-view
</template>

<script>
import UploadButtonComponent from "@/components/upload-button.vue";

export default {
  components: {
    "c-upload-button": UploadButtonComponent,
  },
};
</script>

<style lang="stylus">
#app
  font-family: Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing: antialiased
  -moz-osx-font-smoothing: grayscale
</style>
