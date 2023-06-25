<template lang="pug">
.h-full.w-full.p-5
  .flex.flex-col.h-full.w-full.shadow-xl
    c-video-player.flex-1(:videoUrl="videoUrl")
    div.bg-theme-light.p-5
      h2.font-semibold.text-lg {{ $route.params.id }}
</template>

<script lang="js">
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import VideoPlayerComponent from "@/components/video-player.vue";


export default {
  components: {
    "c-video-player": VideoPlayerComponent,
  },
  computed: {
    videoUrl() {
      return endpointResolver.resolve(`/videos/${this.$route.params.id}`);
    },
  },
};
</script>
