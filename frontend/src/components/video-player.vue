<script setup lang="ts">
import { type Ref, computed, onMounted, onUnmounted, ref } from "vue";
import { useRoute } from "vue-router";
import videojs from "video.js";
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
import { panic } from "@/utils/panic";

const route = useRoute();
const videoId = route.params.id as VideoId;

const videoUrl = computed(() => videoStream(videoId));

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

  const response = await postView(videoId);

  if (!response.success()) {
    console.error("unable to count view: ", response.err());
    return;
  }

  conditionsTracker.viewClaimed = true;
}

const playerElement: Ref<HTMLVideoElement | null> = ref(null);

onMounted(async () => {
  const player = videojs(playerElement.value ?? panic("player is null"), {
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
    setVolume(player.volume());
  });

  const response = await getWatchConditions(videoId);

  if (!response.success()) {
    return;
  }

  watchConditions = response.value();

  timer = new CallbackTimer(watchConditions.remainingTimeMs, () => {
    conditionsTracker.completedTime = true;
    checkWatchConditions();
  });
  player.on("play", () => getValue(timer).resume());
  player.on("pause", () => getValue(timer).pause());
  player.on("progress", () => {
    conditionsTracker.percentage = player.bufferedPercent();
    checkWatchConditions();
  });

  if (!player.paused()) {
    timer.resume();
  }
});

onUnmounted(() => tryApply(timer, (timer) => timer.cancel()));
</script>

<template lang="pug">
div
  video.video-js.vjs-theme-sea(ref="playerElement")
    source(:src="videoUrl" type="video/mp4")
</template>

<style lang="stylus">
$background-fill = rgba(65, 118, 188, .9)
$foreground-fill = hsla(0, 0%, 100%, .5)

.vjs-theme-sea .vjs-big-play-button {
  width: 103px;
  height: 79px;
  -o-object-fit: contain;
  object-fit: contain;
  background-color: hsla(0, 0%, 100%, .25);
  border: none;
  line-height: 79px;
  top: 50%;
  left: 50%;
  border-radius: 30px;
  margin: -51.5px auto 0 -39.5px
}

.vjs-theme-sea .vjs-control-bar {
  height: 4em;
  background-color: hsla(0, 0%, 100%, .4)
}

.vjs-theme-sea .vjs-button:hover {
  color: #4176bc;
  background: linear-gradient(0deg, #d0ddee, #fff)
}

.vjs-theme-sea .vjs-button > .vjs-icon-placeholder:before {
  line-height: 2.2
}

.vjs-theme-sea .vjs-time-control {
  line-height: 4em
}

.vjs-theme-sea .vjs-picture-in-picture-control {
  display: none
}

.vjs-theme-sea .vjs-volume-panel
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

.vjs-theme-sea .vjs-fullscreen-control {
  order: 2
}

.vjs-theme-sea .vjs-progress-control .vjs-play-progress {
  background-color: $background-fill
}

.vjs-theme-sea .vjs-progress-control .vjs-play-progress:before {
  display: none
}

.vjs-theme-sea .vjs-progress-control .vjs-slider {
  background-color: rgba(65, 118, 188, .1)
}

.vjs-theme-sea .vjs-progress-control .vjs-load-progress div
  background: $foreground-fill

.vjs-theme-sea .vjs-progress-control .vjs-progress-holder
  margin: 0
  height: 100%

.vjs-theme-sea .vjs-progress-control .vjs-time-tooltip
  background-color: rgba(65, 118, 188, .5)
  color: #fff

.vjs-theme-sea .vjs-progress-control .vjs-mouse-display .vjs-time-tooltip
  background-color: hsla(0, 0%, 100%, .7)
  color: #4176bc

.vjs-tech:focus
  outline: none
</style>
