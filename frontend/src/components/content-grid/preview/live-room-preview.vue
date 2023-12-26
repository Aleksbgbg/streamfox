<script setup lang="ts">
import CPreviewGeneric from "@/components/content-grid/preview/generic.vue";
import { type LiveRoomInfo, liveRoomThumbnail } from "@/endpoints/live";

defineProps<{
  room: LiveRoomInfo;
}>();
</script>

<template lang="pug">
c-preview-generic(
  :link="{ name: 'live-room', params: { id: room.id } }"
  :thumbnailUrl="liveRoomThumbnail(room.id)"
  :name="room.name"
  :creator="room.creator"
  :beginAt="room.createdAt"
  :viewership="room.participants"
  viewershipName="participant"
)
</template>
