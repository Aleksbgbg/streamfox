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
    videojs(this.$refs.player, {
      autoplay: true,
      controls: true,
      loop: true,
      preload: "auto",
      responsive: true,
      fill: true
    });
  }
};
</script>

<style lang="stylus">
.vjs-tech:focus
  outline: none
</style>