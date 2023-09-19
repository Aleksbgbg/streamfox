<script setup lang="ts">
import { type Ref, ref } from "vue";
import { useRoute } from "vue-router";
import CUserBadge from "@/components/user/badge.vue";
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
.flex.flex-col.h-full.overflow-hidden
  c-video-player.flex-1
  .bg-polar-dark.w-full.p-5(v-if="hasValue(video)")
    .flex.w-full
      h2.flex-1.font-semibold.text-lg {{ getValue(video).name }}
      p {{ getValue(video).views }} {{ getValue(video).views === 1 ? 'view' : 'views' }}
    c-user-badge.mt-2(:user="getValue(video).creator")
    p.mt-2(v-if="getValue(video).description.length > 0") {{ getValue(video).description }}
</template>
