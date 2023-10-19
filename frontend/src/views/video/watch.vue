<script setup lang="ts">
import { type Ref, onBeforeMount, ref } from "vue";
import CVideoPlayer from "@/components/video-player.vue";
import { type VideoId, type VideoInfo, getVideoInfo } from "@/endpoints/video";
import { type Optional, empty, getValue, hasValue } from "@/types/optional";
import CVideoInfo from "@/views/video/info.vue";

const props = defineProps<{
  id: VideoId;
}>();

const video: Ref<Optional<VideoInfo>> = ref(empty());

onBeforeMount(async () => {
  const response = await getVideoInfo(props.id);

  if (!response.success()) {
    return;
  }

  video.value = response.value();
});
</script>

<template lang="pug">
.flex.flex-col.h-full.overflow-hidden
  c-video-player.flex-1(:id="id")
  c-video-info(v-if="hasValue(video)" :video="getValue(video)")
</template>
