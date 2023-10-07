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
.grid.gap-4.m-6(class="grid-cols-[repeat(auto-fill,_minmax(416px,_1fr))]")
  c-video-preview(v-for="video of videos" :video="video")
</template>
