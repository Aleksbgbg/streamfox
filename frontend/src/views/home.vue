<script setup lang="ts">
import { type Ref, onBeforeMount, ref } from "vue";
import CVideoPreview from "@/components/video-preview.vue";
import { type VideoInfo, getVideos } from "@/endpoints/video";

const videos: Ref<VideoInfo[]> = ref([]);

onBeforeMount(async () => {
  const response = await getVideos();

  if (!response.success()) {
    return;
  }

  videos.value = response.value();
});
</script>

<template lang="pug">
.grid.video-grid.gap-4.m-6
  div(v-for="video of videos")
    c-video-preview(:video="video")
</template>

<style lang="stylus" scoped>
.video-grid
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr))
</style>
