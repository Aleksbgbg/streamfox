<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import CVideoPlayer from "@/components/video-player.vue";

const route = useRoute();

const videoUrl = computed(() => endpointResolver.resolve(`/videos/${route.params.id}/stream`));
</script>

<template lang="pug">
.h-full.w-full.p-5
  .flex.flex-col.h-full.w-full.shadow-xl
    c-video-player.flex-1(:videoUrl="videoUrl")
    .bg-theme-light.p-5
      h2.font-semibold.text-lg {{ $route.params.id }}
</template>
