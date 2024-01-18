<script setup lang="ts">
import { computed } from "vue";
import CPreviewGeneric from "@/components/content-grid/preview/generic.vue";
import CIcon from "@/components/icon.vue";
import { type VideoInfo, Visibility, videoThumbnail } from "@/endpoints/video";

const props = defineProps<{
  showVisibility: boolean;
  video: VideoInfo;
}>();

const icon = computed(() => {
  switch (props.video.visibility) {
    case Visibility.Private:
      return "lock-fill";
    case Visibility.Unlisted:
      return "eye-slash-fill";
    case Visibility.Public:
      return "eye-fill";
    default:
      return "";
  }
});
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
  c-icon.text-slate-300(v-if="showVisibility" :name="icon")
</template>
