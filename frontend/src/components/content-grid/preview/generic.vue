<script setup lang="ts">
import type { RouteLocationRaw } from "vue-router";
import type { User } from "@/endpoints/user";
import { dateToElapsedTimeString, secsToDurationString } from "@/utils/strings";

defineProps<{
  link: RouteLocationRaw;
  thumbnailUrl: string;
  durationSecs: number;
  name: string;
  creator: User;
  beginAt: Date;
  viewership: number;
  viewershipName: string;
}>();
</script>

<template lang="pug">
router-link.group.w-full(class="max-w-[416px]" :to="link")
  .relative.aspect-video
    img(src="@/assets/fox.png" alt="")
    img.absolute.top-0(:src="thumbnailUrl" alt="")
    span(
      class="absolute bottom-0.5 right-0.5 text-sm opacity-85 bg-black px-1"
    ) {{ secsToDurationString(durationSecs) }}
  h3.break-words.line-clamp-2.font-semibold.mt-2(
    class="group-hover:underline group-hover:text-aurora-yellow"
  ) {{ name }}
  p.text-sm {{ creator.username }}
  p.text-sm
    | {{ viewership }} {{ viewershipName + (viewership === 1 ? '' : 's') }}
    | •
    | {{ dateToElapsedTimeString(beginAt) }}
</template>
