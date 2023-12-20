<script setup lang="ts">
import { type Ref, computed, onMounted, onUnmounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import videojs from "video.js";
import type Player from "video.js/dist/types/player";
import CContextMenuItem from "@/components/menu/context-menu-item.vue";
import CContextMenu from "@/components/menu/context-menu.vue";
import { getAllSubtitles, subtitleContentUrl } from "@/endpoints/subtitle";
import {
  type VideoId,
  type WatchConditions,
  getWatchConditions,
  postView,
  videoStream,
} from "@/endpoints/video";
import { getVolume, setVolume } from "@/settings/volume";
import { type Optional, empty, getValue, tryApply } from "@/types/optional";
import { CallbackTimer } from "@/utils/callback-timer";
import { clipboardCopy } from "@/utils/clipboard";
import { panic } from "@/utils/panic";
import { fullUrl } from "@/utils/url";

const router = useRouter();
const route = useRoute();

const props = defineProps<{
  id: VideoId;
}>();

const videoUrl = computed(() => videoStream(props.id));

let watchConditions: Optional<WatchConditions> = empty();
const conditionsTracker = {
  viewClaimed: false,
  percentage: 0,
  completedTime: false,
};
let timer: Optional<CallbackTimer> = empty();

async function checkWatchConditions() {
  if (
    conditionsTracker.viewClaimed ||
    conditionsTracker.percentage < getValue(watchConditions).percentage ||
    !conditionsTracker.completedTime
  ) {
    return;
  }

  const response = await postView(props.id);

  if (!response.success()) {
    console.error("unable to count view: ", response.err());
    return;
  }

  conditionsTracker.viewClaimed = true;
}

const playerElement: Ref<HTMLVideoElement | null> = ref(null);

function handleSpecificTimestamp(player: Player) {
  if (route.query.t) {
    player.load();

    const skipForwardSecs = parseInt(route.query.t as string);
    player.currentTime((player.currentTime() ?? 0) + skipForwardSecs);
  }
}

let player: Optional<Player> = empty();
onMounted(async () => {
  player = videojs(playerElement.value ?? panic("player is null"), {
    autoplay: true,
    controls: true,
    loop: true,
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
    setVolume(getValue(player).volume() ?? 0.5);
  });

  const [watch, subs] = await Promise.all([
    getWatchConditions(props.id),
    getAllSubtitles(props.id),
  ]);

  if (watch.success()) {
    watchConditions = watch.value();

    timer = new CallbackTimer(watchConditions.remainingTimeMs, () => {
      conditionsTracker.completedTime = true;
      checkWatchConditions();
    });
    player.on("play", () => getValue(timer).resume());
    player.on("pause", () => getValue(timer).pause());
    player.on("progress", () => {
      conditionsTracker.percentage = getValue(player).bufferedPercent();
      checkWatchConditions();
    });

    if (!player.paused()) {
      timer.resume();
    }
  }

  handleSpecificTimestamp(player);

  if (subs.success()) {
    for (const subtitle of subs.value()) {
      player.addRemoteTextTrack({
        kind: "subtitles",
        label: subtitle.name,
        srclang: undefined,
        src: subtitleContentUrl(props.id, subtitle.id),
        default: false,
      });
    }
  }
});

onUnmounted(() => tryApply(timer, (timer) => timer.cancel()));

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
        name: getValue(route.name),
        query: { t: Math.round(player?.currentTime() || 0).toString() },
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
