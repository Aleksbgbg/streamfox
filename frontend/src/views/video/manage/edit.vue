<script setup lang="ts">
import CFormLayout from "@/components/layout/form.vue";
import { type VideoId } from "@/endpoints/video";
import CEditDetails from "@/views/video/manage/details.vue";

defineProps<{
  videoId: VideoId;
}>();
</script>

<template lang="pug">
c-form-layout(title="Edit Video")
  c-edit-details(:video-id="videoId")
</template>
