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
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import { videoLister } from "@/bootstrapper/video-endpoint";
import WatchLinkComponent from "@/components/home/watch-link.vue";

export default {
  components: {
    "c-watch-link": WatchLinkComponent,
  },
  data() {
    return {
      videos: [],
    };
  },
  async created() {
    const videoList = await videoLister.listVideos();

    for (const { id } of videoList.reverse()) {
      this.videos.push({
        id,
        thumbnail: await endpointResolver.resolve(`/videos/${id}/thumbnail`),
      });
    }
  },
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
