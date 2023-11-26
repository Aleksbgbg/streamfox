<script setup lang="ts">
import { type Ref, onBeforeMount, ref } from "vue";
import { useToaster } from "@/components/toasts/toaster";
import CVideoPreview from "@/components/video-preview.vue";
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
.flex.flex-col.justify-items-center.gap-4.m-6(class="min-[368px]:grid grid-cols-[repeat(auto-fill,_minmax(320px,_1fr))]")
  c-video-preview(class="max-w-[416px]" v-for="video of videos" :video="video")
</template>
