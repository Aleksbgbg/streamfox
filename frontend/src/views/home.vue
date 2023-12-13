<script setup lang="ts">
import { type Ref, onBeforeMount, ref } from "vue";
import CVideoPreview from "@/components/content-grid/preview/video-preview.vue";
import CTileGrid from "@/components/content-grid/tile-grid.vue";
import CEmptyCollection from "@/components/empty-collection.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type VideoInfo, getVideos } from "@/endpoints/video";

const toaster = useToaster();

const videos: Ref<VideoInfo[]> = ref([]);

onBeforeMount(async () => {
  const response = await getVideos();

  if (!response.success()) {
    toaster.failure("Unable to fetch videos.");
    return;
  }

  videos.value = response.value();
});
</script>

<template lang="pug">
c-empty-collection(:collection="videos" empty="No videos yet. Upload one!" text-margin)
  c-tile-grid
    c-video-preview(v-for="video of videos" :video="video")
</template>
