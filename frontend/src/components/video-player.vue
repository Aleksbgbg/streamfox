<script setup lang="ts">
import { type Ref, computed, onMounted, onUnmounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import videojs from "video.js";
import type Player from "video.js/dist/types/player";
import CContextMenuItem from "@/components/menu/context-menu-item.vue";
import CContextMenu from "@/components/menu/context-menu.vue";
import { getAllSubtitles, subtitleContentUrl } from "@/endpoints/subtitle";
import { type VideoId, postWatchHint, videoStream } from "@/endpoints/video";
import { getLoop, setLoop } from "@/settings/loop";
import { getVolume, setVolume } from "@/settings/volume";
import { ContinuousCallbackTimer } from "@/utils/callback-timer";
import { clipboardCopy } from "@/utils/clipboard";
import { check } from "@/utils/null";
import { once } from "@/utils/once";
import { fullUrl } from "@/utils/url";

const router = useRouter();
const route = useRoute();

const props = defineProps<{
  id: VideoId;
}>();

const videoUrl = computed(() => videoStream(props.id));

let loop = ref(getLoop());
watch(loop, setLoop);

const playerElement: Ref<HTMLVideoElement | null> = ref(null);

function handleSpecificTimestamp(player: Player) {
  if (route.query.t) {
    player.load();

    const skipForwardSecs = parseInt(route.query.t as string);
    player.currentTime((player.currentTime() ?? 0) + skipForwardSecs);
  }
}

let player: Player;
let timer: ContinuousCallbackTimer;
onMounted(async () => {
  player = videojs(check(playerElement.value), {
    autoplay: true,
    controls: true,
    loop: false,
    preload: "auto",
    responsive: true,
    fill: true,
    controlBar: {
      volumePanel: {
        vertical: true,
        inline: false,
      },
    },
  });

  player.volume(getVolume());
  player.on("volumechange", function () {
    setVolume(player.volume() ?? 0.5);
  });

  timer = new ContinuousCallbackTimer(30_000, () => postWatchHint(props.id));
  const watchHintOnStart = once(() => postWatchHint(props.id));
  player.on("play", () => {
    watchHintOnStart();
    timer.resume();
  });
  player.on("pause", () => timer.pause());
  player.on("ended", () => {
    postWatchHint(props.id);

    if (loop.value) {
      player.play();
    }
  });

  handleSpecificTimestamp(player);

  const subs = await getAllSubtitles(props.id);
  if (!subs.success()) {
    return;
  }
  for (const subtitle of subs.value()) {
    player.addRemoteTextTrack({
      kind: "subtitles",
      label: subtitle.name,
      srclang: undefined,
      src: subtitleContentUrl(props.id, subtitle.id),
      default: false,
    });
  }
});

onUnmounted(() => timer.cancel());

const contextMenu: Ref<typeof CContextMenu | null> = ref(null);
function onContextMenu(event: MouseEvent) {
  contextMenu.value?.show({ x: event.clientX, y: event.clientY });
}
function copyUrl() {
  clipboardCopy(fullUrl(route.path));
}
function copyUrlTimestamp() {
  clipboardCopy(
    fullUrl(
      router.resolve({
        name: check(route.name),
        query: { t: Math.round(player.currentTime() || 0).toString() },
      }).fullPath,
    ),
  );
}
</script>

<template lang="pug">
div(data-vjs-player)
  video.video-js.vjs-theme-sea(
    ref="playerElement"
    @contextmenu.prevent="onContextMenu"
  )
    source(:src="videoUrl" type="video/mp4")
  c-context-menu(ref="contextMenu")
    c-context-menu-item(
      :icon="loop ? 'check' : 'arrow-counterclockwise'"
      @click="loop = !loop"
    ) Loop
    c-context-menu-item(icon="link" @click="copyUrl") Copy video URL
    c-context-menu-item(icon="link" @click="copyUrlTimestamp") Copy video URL at current time
</template>

<style lang="stylus">
$background-fill = rgba(65, 118, 188, .9)
$foreground-fill = hsla(0, 0%, 100%, .5)

.vjs-theme-sea
  .vjs-big-play-button
    width: 103px
    height: 79px
    -o-object-fit: contain
    object-fit: contain
    background-color: hsla(0, 0%, 100%, .25)
    border: none
    line-height: 79px
    top: 50%
    left: 50%
    border-radius: 30px
    margin: -51.5px auto 0 -39.5px

  .vjs-control-bar
    height: 4em
    background-color: hsla(0, 0%, 100%, .4)

  .vjs-button
    &:hover
      color: #4176bc
      background: linear-gradient(0deg, #d0ddee, #fff)

    & > .vjs-icon-placeholder:before
      line-height: 2.2

  .vjs-time-control
    line-height: 4em

  .vjs-subs-caps-button .vjs-menu-content
    margin-bottom: 10px

  .vjs-picture-in-picture-control
    display: none

  .vjs-volume-panel
    order: 1

    .vjs-volume-vertical
      margin-top: -0.25em

    .vjs-slider-bar
      margin: 0

    .vjs-volume-bar
      height: 100%
      width: 100%
      background-color: $foreground-fill

      .vjs-volume-level
        width: 100%
        background-color: $background-fill

        &:before
          display: none

  .vjs-fullscreen-control
    order: 2

  .vjs-progress-control
    .vjs-play-progress
      background-color: $background-fill

      &:before
        display: none

    .vjs-slider
      background-color: rgba(65, 118, 188, .1)

    .vjs-load-progress div
      background: $foreground-fill

    .vjs-progress-holder
      margin: 0
      height: 100%

    .vjs-time-tooltip
      background-color: rgba(65, 118, 188, .5)
      color: #fff

    .vjs-mouse-display .vjs-time-tooltip
      background-color: hsla(0, 0%, 100%, .7)
      color: #4176bc

.vjs-tech:focus
  outline: none
</style>
