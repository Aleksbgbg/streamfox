<script setup lang="ts">
import CFormLayout from "@/components/layout/form.vue";
import CTab from "@/components/tab-control/tab.vue";
import CTabs from "@/components/tab-control/tabs.vue";
import { type VideoId } from "@/endpoints/video";
import CEditDetails from "@/views/video/manage/details.vue";
import CEditSubtitles from "@/views/video/manage/subtitles.vue";

defineProps<{
  videoId: VideoId;
}>();
</script>

<template lang="pug">
c-form-layout(title="Edit Video")
  c-tabs
    c-tab(title="Details")
      c-edit-details(:video-id="videoId")
    c-tab(title="Subtitles")
      c-edit-subtitles(:video-id="videoId")
</template>
