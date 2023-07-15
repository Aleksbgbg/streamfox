<script setup lang="ts">
import { onBeforeMount, reactive } from "vue";
import CWatchLink from "@/components/watch-link.vue";
import { type VideoInfo, getVideos, videoThumbnail } from "@/endpoints/video";

interface Video {
  info: VideoInfo;
  thumbnail: string;
}

const videos: Video[] = reactive([]);

onBeforeMount(async () => {
  for (const info of await getVideos()) {
    videos.push({
      info,
      thumbnail: videoThumbnail(info.id),
    });
  }
});
</script>

<template lang="pug">
.grid.video-grid.gap-4.m-6
  div(v-for="video of videos")
    .flex.flex-col.items-center
      c-watch-link(:videoId="video.info.id")
        img(:src="video.thumbnail" alt="").max-res-225p
      c-watch-link(
        class="text-blue-100 hover:text-blue-200 no-underline text-lg hover:underline"
        :videoId="video.info.id"
      ) {{ video.info.id }}
</template>

<style lang="stylus" scoped>
.video-grid
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr))

.max-res-225p
  max-width: 400px
  max-height: 225px
  width: auto
  height: auto
</style>
