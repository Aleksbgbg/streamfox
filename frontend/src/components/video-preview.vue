<script setup lang="ts">
import CWatchLink from "@/components/watch-link.vue";
import { type VideoInfo, videoThumbnail } from "@/endpoints/video";
import { dateToElapsedTimeString, secsToDurationString } from "@/utils/strings";

const props = defineProps<{
  video: VideoInfo;
}>();
</script>

<template lang="pug">
.flex.justify-center
  c-watch-link.group(class="w-[416px]" :videoId="props.video.id")
    .relative
      img(:src="videoThumbnail(props.video.id)" alt="")
      span(
        class="absolute bottom-0.5 right-0.5 text-sm opacity-85 bg-black px-1"
      ) {{ secsToDurationString(props.video.durationSecs) }}
    h3.break-words.line-clamp-2.font-semibold.mt-2(
      class="group-hover:underline group-hover:text-aurora-yellow"
    ) {{ props.video.name }}
    p.text-sm {{ props.video.creator.username }}
    p.text-sm
      | {{ props.video.views }} {{ props.video.views === 1 ? 'view' : 'views' }}
      | •
      | {{ dateToElapsedTimeString(props.video.uploadedAt) }}
</template>
