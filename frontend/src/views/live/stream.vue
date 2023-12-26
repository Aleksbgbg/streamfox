<script setup lang="ts">
import { type Ref, onMounted, ref, watch } from "vue";
import CSpinner from "@/components/spinner.vue";
import { check } from "@/utils/null";

interface Props {
  stream: MediaStream;
  mainView?: boolean;
  focused?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
  mainView: false,
  focused: false,
});

const loading = ref(true);

const video: Ref<HTMLVideoElement | null> = ref(null);

function setStream() {
  loading.value = true;
  check(video.value).srcObject = props.stream;
}

onMounted(() => {
  check(video.value).addEventListener("canplay", function () {
    loading.value = false;
  });

  setStream();
});

watch(() => props.stream, setStream);
</script>

<template lang="pug">
.relative.w-full.h-full
  video.w-full.h-full(
    :class="{ 'blur-sm': focused && !mainView }"
    ref="video"
    autoplay
    :controls="mainView"
    :muted="!mainView"
  )
  .absolute.left-0.top-0.flex.items-center.justify-center.w-full.h-full(
    v-show="loading"
  )
    c-spinner
</template>
