<template lang="pug">
video.video-js.vjs-theme-sea(ref="player")
  source(:src="videoUrl" type="video/mp4")
</template>

<script>
import videojs from "video.js";

export default {
  props: {
    videoUrl: String
  },
  mounted() {
    const player = videojs(this.$refs.player, {
      autoplay: true,
      controls: true,
      loop: true,
      preload: "auto",
      responsive: true,
      fill: true
    });
    player.volume(0.5);
  }
};
</script>

<style lang="stylus">
.vjs-tech:focus
  outline: none
</style>