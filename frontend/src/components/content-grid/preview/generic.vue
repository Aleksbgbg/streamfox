<script setup lang="ts">
import type { RouteLocationRaw } from "vue-router";
import CAnchorCover from "@/components/anchor/cover.vue";
import CAnchorRoot from "@/components/anchor/root.vue";
import type { User } from "@/endpoints/user";
import { dateToElapsedTimeString, secsToDurationString } from "@/utils/strings";

defineProps<{
  link: RouteLocationRaw;
  thumbnailUrl: string;
  durationSecs?: number;
  name: string;
  creator: User;
  beginAt: Date;
  viewership: number;
  viewershipName: string;
}>();
</script>

<template lang="pug">
c-anchor-root.group.w-full(class="max-w-[416px]")
  .relative.aspect-video
    img(src="@/assets/fox.png" alt="")
    img.absolute.top-0(:src="thumbnailUrl" alt="")
    span(
      v-if="durationSecs"
      class="absolute bottom-0.5 right-0.5 text-sm opacity-85 bg-black px-1"
    ) {{ secsToDurationString(durationSecs) }}
  .flex.justify-between.gap-2.mt-2
    .min-w-0
      router-link(:to="link")
        h3.break-words.line-clamp-2.font-semibold(
          class="group-hover:underline group-hover:text-aurora-yellow"
        ) {{ name }}
        c-anchor-cover
      p.text-sm {{ creator.username }}
      p.text-sm
        | {{ viewership }} {{ viewershipName + (viewership === 1 ? '' : 's') }}
        | •
        | {{ dateToElapsedTimeString(beginAt) }}
    slot
</template>
