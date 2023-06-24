<template lang="pug">
div
  div.grid.video-grid.gap-4.m-6
    div(v-for="video of videos")
      div.flex.flex-col.items-center
        c-watch-link(:videoId="video.id")
          img.max-res-225p(:src="video.thumbnail" alt="")
        c-watch-link.text-blue-100.no-underline.hover-underline.hover-text-blue-200.text-lg(:videoId="video.id") {{ video.id }}
</template>

<script>
import WatchLinkComponent from "@/components/home/watch-link.vue";
import { videoLister } from "@/bootstrapper/video-endpoint";
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";

export default {
  components: {
    "c-watch-link": WatchLinkComponent
  },
  data() {
    return {
      videos: []
    };
  },
  async created() {
    const videoList = await videoLister.listVideos();

    for (const videoId of videoList.videoIds.reverse()) {
      this.videos.push({
        id: videoId,
        thumbnail: await endpointResolver.resolve(`/videos/${videoId}/thumbnail`)
      });
    }
  }
};
</script>

<style lang="stylus" scoped>
.video-grid
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr))

.max-res-225p
  max-width: 400px
  max-height: 225px
  width: auto
  height: auto
</style>