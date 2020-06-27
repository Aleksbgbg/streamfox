<template lang="pug">
.flex.flex-col.items-center.h-full.w-full
  .shadow-xl.h-full.w-full.max-res-720p.m-5
    video(
      id="my-video"
      class="video-js"
      controls
      preload="auto"
      data-setup="{}"
    )
      source(:src="videoUrl" type="video/mp4")
    div.bg-theme-light.p-5
      h2.font-semibold.text-lg {{ $route.params.id }}
</template>

<script>
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";

export default {
  computed: {
    videoUrl() {
      return endpointResolver.resolve(`/videos/${this.$route.params.id}`);
    }
  }
};
</script>

<style lang="stylus" scoped>
.max-res-720p
  max-width: 1280px
  max-height: 720px
  width: auto
  height: auto
</style>