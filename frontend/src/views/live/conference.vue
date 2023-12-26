<script setup lang="ts">
import { onBeforeMount, onUnmounted, reactive, shallowRef } from "vue";
import CButton from "@/components/button.vue";
import CIcon from "@/components/icon.vue";
import { useToaster } from "@/components/toasts/toaster";
import CUserAvatar from "@/components/user/avatar.vue";
import { type LiveRoomInfo, joinLiveRoom } from "@/endpoints/live";
import { type User, type UserId } from "@/endpoints/user";
import { none, some } from "@/types/option";
import { getStrict } from "@/utils/maps";
import CStream from "@/views/live/stream.vue";

const toaster = useToaster();

const emit = defineEmits<{
  (e: "left"): void;
}>();

const props = defineProps<{
  room: LiveRoomInfo;
}>();

const users = reactive(new Map<string, User>());
const streams = reactive(new Map<string, MediaStream>());
const activePresenter = shallowRef(none<UserId>());

let peerConnection: RTCPeerConnection;

const transceiverToUser = new Map<RTCRtpTransceiver, string>();
const userToTransceivers = new Map<string, [RTCRtpTransceiver, RTCRtpTransceiver]>();

onBeforeMount(() => {
  peerConnection = joinLiveRoom(props.room.id, {
    connected(payload) {
      users.set(payload.user.id, payload.user);
    },
    disconnected(payload) {
      users.delete(payload.user.id);
      streams.delete(payload.user.id);
    },
    startStream(payload) {
      streams.set(payload.userId, new MediaStream());
      const videoTransceiver = peerConnection.addTransceiver("video", { direction: "recvonly" });
      const audioTransceiver = peerConnection.addTransceiver("audio", { direction: "recvonly" });
      transceiverToUser.set(videoTransceiver, payload.userId);
      transceiverToUser.set(audioTransceiver, payload.userId);
      userToTransceivers.set(payload.userId, [videoTransceiver, audioTransceiver]);
    },
    stopStream(payload) {
      if (activePresenter.value.eq(some(payload.userId))) {
        activePresenter.value = none();
      }

      const [videoTransceiver, audioTransceiver] = getStrict(userToTransceivers, payload.userId);
      userToTransceivers.delete(payload.userId);
      transceiverToUser.delete(audioTransceiver);
      transceiverToUser.delete(videoTransceiver);
      audioTransceiver.stop();
      videoTransceiver.stop();
      streams.delete(payload.userId);
    },
    closed() {
      emit("left");
    },
    error(messages) {
      toaster.failureAll(messages);
    },
  });
  peerConnection.addEventListener("track", function (event) {
    const userId = getStrict(transceiverToUser, event.transceiver);
    getStrict(streams, userId).addTrack(event.track);

    if (activePresenter.value.isNone()) {
      activePresenter.value = some(userId);
    }
  });
});

onUnmounted(() => {
  peerConnection.close();
});

function isActivePresenter(id: string): boolean {
  return activePresenter.value.eq(some(id));
}

function togglePresenter(id: string) {
  if (isActivePresenter(id)) {
    activePresenter.value = none();
  } else {
    activePresenter.value = some(id);
  }
}
</script>

<template lang="pug">
.flex.flex-col.bg-polar-light.h-full
  h2.bg-polar-dark.text-center.py-2 {{ room.name }}
  .grow.min-w-0.min-h-0
    c-stream(
      v-if="activePresenter.isSome()"
      :stream="getStrict(streams, activePresenter.get())"
      main-view
    )
  .flex.gap-3.bg-polar-darkest.p-5.overflow-x-auto.h-36
    .relative.group.shrink-0.flex.items-center.justify-center.rounded-lg.bg-polar-light.aspect-video.overflow-hidden(
      v-for="[id, user] of users"
      :key="id"
      :class="{ 'ring-2 ring-frost-blue': isActivePresenter(id) }"
    )
      template(v-if="streams.has(id)")
        c-stream(:stream="getStrict(streams, id)" :focused="isActivePresenter(id)")
        c-button.absolute.invisible(
          class="group-hover:visible"
          theme="invisible"
          @click="togglePresenter(id)"
        )
          c-icon(:name="isActivePresenter(id) ? 'camera-video-off' : 'camera-video'")
      template(v-else)
        c-user-avatar.w-12(:user="user")
      p.absolute.left-0.bottom-0.text-stroke.truncate.p-2 {{ user.username }}
</template>

<style lang="stylus" scoped>
.text-stroke
  text-shadow: -1px -1px 0 black, 1px -1px 0 black, -1px 1px 0 black, 1px 1px 0 black
</style>
