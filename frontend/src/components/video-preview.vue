<script setup lang="ts">
import CWatchLink from "@/components/watch-link.vue";
import { type VideoInfo, videoThumbnail } from "@/endpoints/video";
import { dateToElapsedTimeString, secsToDurationString } from "@/utils/strings";

defineProps<{
  video: VideoInfo;
}>();
</script>

<template lang="pug">
c-watch-link.group.w-full(:videoId="video.id")
  .relative.aspect-video
    img(src="@/assets/fox.png" alt="")
    img.absolute.top-0(:src="videoThumbnail(video.id)" alt="")
    span(
      class="absolute bottom-0.5 right-0.5 text-sm opacity-85 bg-black px-1"
    ) {{ secsToDurationString(video.durationSecs) }}
  h3.break-words.line-clamp-2.font-semibold.mt-2(
    class="group-hover:underline group-hover:text-aurora-yellow"
  ) {{ video.name }}
  p.text-sm {{ video.creator.username }}
  p.text-sm
    | {{ video.views }} {{ video.views === 1 ? 'view' : 'views' }}
    | •
    | {{ dateToElapsedTimeString(video.uploadedAt) }}
</template>
