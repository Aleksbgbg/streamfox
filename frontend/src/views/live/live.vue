<script setup lang="ts">
import { type Ref, onBeforeMount, ref } from "vue";
import CButton from "@/components/button.vue";
import CLiveRoomPreview from "@/components/content-grid/preview/live-room-preview.vue";
import CTileGrid from "@/components/content-grid/tile-grid.vue";
import CEmptyCollection from "@/components/empty-collection.vue";
import CIcon from "@/components/icon.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type LiveRoomInfo, getLiveRooms } from "@/endpoints/live";

const toaster = useToaster();

const rooms: Ref<LiveRoomInfo[]> = ref([]);

onBeforeMount(async () => {
  const response = await getLiveRooms();

  if (!response.success()) {
    toaster.failure("Unable to fetch live rooms.");
    return;
  }

  rooms.value = response.value();
});
</script>

<template lang="pug">
c-empty-collection(
  :collection="rooms"
  empty="No rooms currently available. Create one!"
  text-margin
)
  c-tile-grid
    c-live-room-preview(v-for="room of rooms" :room="room")
.absolute.left-3.bottom-3.flex.flex-col.gap-3
  c-button.flex.items-center(
    element="router-link"
    theme="blue"
    :to="{ name: 'setup-obs' }"
  )
    img.inline.w-5.h-5.mr-1(class="-ml-0.5" src="@/assets/obs-logo.png" alt="OBS logo")
    span Setup OBS
  c-button(
    element="router-link"
    theme="red"
    :to="{ name: 'create-live-room' }"
  )
    c-icon.text-white-500.mr-2.animate-pulse(name="circle-fill")
    span Go Live
</template>
