<script setup lang="ts">
import CButton from "@/components/button.vue";
import CLiveRoomPreview from "@/components/content-grid/preview/live-room-preview.vue";
import CFormLayout from "@/components/layout/form.vue";
import { type LiveRoomInfo } from "@/endpoints/live";

defineEmits<{
  (e: "joined"): void;
}>();

defineProps<{
  room: LiveRoomInfo;
  joinedPreviously: boolean;
}>();
</script>

<template lang="pug">
c-form-layout(:title='`Join "${room.name}"?`')
  form.flex.flex-col.items-center.gap-3.my-5(@submit.prevent="$emit('joined')")
    c-live-room-preview(:room="room")
    p.text-center.px-5(v-if="joinedPreviously")
      | You disconnected from the room. This could happen when you join the same room from
      | multiple tabs, or if you are disconnected from the server. You can join again.
    c-button Join
</template>
