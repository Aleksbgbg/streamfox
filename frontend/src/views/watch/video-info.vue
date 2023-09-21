<script setup lang="ts">
import CUserBadge from "@/components/user/badge.vue";
import type { VideoInfo } from "@/endpoints/video";

const props = defineProps<{
  video: VideoInfo;
}>();
</script>

<template lang="pug">
.bg-polar-dark.w-full.p-5
  .flex.w-full
    h2.flex-1.font-semibold.text-lg {{ props.video.name }}
    p {{ props.video.views }} {{ props.video.views === 1 ? 'view' : 'views' }}
  c-user-badge.mt-2(:user="props.video.creator")
  p.mt-2(v-if="props.video.description.length > 0") {{ props.video.description }}
</template>
