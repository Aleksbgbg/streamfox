<script setup lang="ts">
import { onBeforeMount, ref, shallowRef } from "vue";
import { useRouter } from "vue-router";
import { useToaster } from "@/components/toasts/toaster";
import { type LiveRoomId, type LiveRoomInfo, getLiveRoom } from "@/endpoints/live";
import { none, some } from "@/types/option";
import CConference from "@/views/live/conference.vue";
import CJoin from "@/views/live/join.vue";

const router = useRouter();

const toaster = useToaster();

const props = defineProps<{
  id: LiveRoomId;
}>();

const showJoinScreen = ref(true);
const joinedPreviously = ref(false);

const room = shallowRef(none<LiveRoomInfo>());

onBeforeMount(async () => {
  const response = await getLiveRoom(props.id);

  if (!response.success()) {
    toaster.failureAll(response.err().generic);
    router.push({ name: "live" });
    return;
  }

  room.value = some(response.value());
});
</script>

<template lang="pug">
template(v-if="showJoinScreen")
  c-join(
    v-if="room.isSome()"
    :room="room.get()"
    :joined-previously="joinedPreviously"
    @joined="showJoinScreen = false; joinedPreviously = true"
  )
template(v-else)
  c-conference(:room="room.get()" @left="showJoinScreen = true")
</template>
