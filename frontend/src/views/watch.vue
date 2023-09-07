<script setup lang="ts">
import { type Ref, ref } from "vue";
import { useRoute } from "vue-router";
import CVideoPlayer from "@/components/video-player.vue";
import { type VideoId, type VideoInfo, getVideoInfo } from "@/endpoints/video";
import { type Optional, empty, getValue, hasValue } from "@/types/optional";

const route = useRoute();
const videoId = route.params.id as VideoId;
const video: Ref<Optional<VideoInfo>> = ref(empty());
getVideoInfo(videoId).then((response) => {
  if (!response.success()) {
    return;
  }

  video.value = response.value();
});
</script>

<template lang="pug">
.h-full.w-full.p-5
  .flex.flex-col.h-full.w-full.shadow-xl
    c-video-player.flex-1
    .flex.bg-theme-light.p-5
      h2.flex-1.font-semibold.text-lg {{ $route.params.id }}
      p(
        v-if="hasValue(video)"
      ) {{ getValue(video).views }} {{ getValue(video).views === 1 ? 'view' : 'views' }}
</template>
