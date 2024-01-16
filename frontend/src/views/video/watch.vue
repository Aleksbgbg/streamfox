<script setup lang="ts">
import { type Ref, onBeforeMount, shallowRef } from "vue";
import CVideoPlayer from "@/components/video-player.vue";
import { type VideoId, type VideoInfo, getVideoInfo } from "@/endpoints/video";
import { type Option, none, some } from "@/types/option";
import CVideoInfo from "@/views/video/info.vue";

const props = defineProps<{
  id: VideoId;
}>();

const video: Ref<Option<VideoInfo>> = shallowRef(none());

onBeforeMount(async () => {
  const response = await getVideoInfo(props.id);

  if (!response.success()) {
    return;
  }

  video.value = some(response.value());
});
</script>

<template lang="pug">
.flex.flex-col.h-full.overflow-hidden
  c-video-player.flex-1(:id="id")
  c-video-info(v-if="video.isSome()" :video="video.get()")
</template>
