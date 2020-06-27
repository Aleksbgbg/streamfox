<template lang="pug">
#app.grid.grid-rows-6.bg-theme-dark.text-white.h-screen
  header.row-end-1.bg-theme-darkest.h-20.p-5.shadow-2xl
    nav.flex
      router-link(:to="{ name: 'home' }")
        h1.font-bold.text-2xl Streamfox
      c-upload-button(@uploadStart="uploadStart" @uploadSuccess="uploadSuccess" @uploadFail="uploadFail")
  main.row-start-1.row-end-7.overflow-auto
    div.grid.grid-cols-2.h-1(v-show="isUploading")
      div.bg-blue-500(:class="['col-end-1', '', 'col-span-2'][uploadStage]")
      div.bg-gray-500(:class="['col-span-2', '', 'col-start-3'][uploadStage]")
    router-view
  div.absolute.h-full.w-full(v-show="showUploadFailed")
    div.absolute.h-full.w-full.bg-black.opacity-50
    div.absolute.h-full.w-full.flex.justify-center.items-start
      div.border.rounded-md.border-red-600.bg-black.text-red-600.p-5.mt-5
        button.focus-outline-none(@click="showUploadFailed = false")
          p.text-3xl Upload failed.
</template>

<script>
import UploadButtonComponent from "@/components/upload-button.vue";

export default {
  components: {
    "c-upload-button": UploadButtonComponent
  },
  data() {
    return {
      isUploading: false,
      showUploadFailed: false,
      uploadStage: 0
    };
  },
  methods: {
    uploadStart() {
      this.uploadStage = 0;
      this.isUploading = true;

      setTimeout(() => {
        this.uploadStage = 1;
      }, 500);
    },
    uploadSuccess(videoId) {
      this.uploadStage = 2;
      this.isUploading = false;

      this.$router.push(`/watch/${videoId}`);
    },
    uploadFail() {
      this.uploadStage = 2;
      this.isUploading = false;
      this.showUploadFailed = true;
    }
  }
};
</script>

<style lang="stylus">
#app
  font-family Avenir, Helvetica, Arial, sans-serif
  -webkit-font-smoothing antialiased
  -moz-osx-font-smoothing grayscale
</style>