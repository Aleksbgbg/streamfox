<script setup lang="ts">
import CPreviewGeneric from "@/components/content-grid/preview/generic.vue";
import { type VideoInfo, videoThumbnail } from "@/endpoints/video";

defineProps<{
  video: VideoInfo;
}>();
</script>

<template lang="pug">
c-preview-generic(
  :link="{ name: 'watch', params: { id: video.id } }"
  :thumbnailUrl="videoThumbnail(video.id)"
  :durationSecs="video.durationSecs"
  :name="video.name"
  :creator="video.creator"
  :beginAt="video.uploadedAt"
  :viewership="video.views"
  viewershipName="view"
)
</template>
