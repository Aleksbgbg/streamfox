<script setup lang="ts">
import { type Ref, onBeforeMount, reactive, ref } from "vue";
import { useRoute } from "vue-router";
import CButton from "@/components/button.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormOption from "@/components/forms/select/option.vue";
import CFormSelect from "@/components/forms/select/select.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import {
  type VideoId,
  type VideoUpdateInfo,
  Visibility,
  getVideoInfo,
  updateVideo,
  videoThumbnail,
} from "@/endpoints/video";

const route = useRoute();

const toaster = useToaster();

const props = defineProps<{
  videoId: VideoId;
}>();

const video: VideoUpdateInfo = reactive({
  name: "",
  description: "",
  visibility: Visibility.Public,
});

const err: Ref<ApiErr<VideoUpdateInfo>> = ref(emptyApiErr());

onBeforeMount(async () => {
  const response = await getVideoInfo(props.videoId);

  if (!response.success()) {
    toaster.failureAll(response.err().generic);
    return;
  }

  const value = response.value();
  video.name = value.name;
  video.description = value.description;
  video.visibility = value.visibility;
});

async function save() {
  const response = await updateVideo(props.videoId, video);

  if (!response.success()) {
    err.value = response.err();
    toaster.failureAll(err.value.generic);
    return;
  }

  err.value = emptyApiErr();
  toaster.success("Video details saved!");
}
</script>

<template lang="pug">
form.flex.flex-col.justify-center.gap-6.h-full.px-5.py-4(class="md:flex-row" @submit.prevent="save")
  .flex.flex-col.items-center(class="md:items-start")
    img.mb-2(class="max-w-[416px]" :src="videoThumbnail(props.videoId)")
    label.font-semibold(for="video-link") Video link
    router-link.break-all.underline.text-frost-cyan.mb-2#video-link(
      :to="{ name: 'watch', params: { id: videoId } }"
    ) watch/{{ videoId }}
    template(v-if="route.query.filename")
      label.font-semibold(for="file") File
      p.break-all#file {{ route.query.filename }}
  .flex-grow.flex.flex-col.items-center.gap-3.min-w-0
    c-form-input(
      title="Name"
      v-model="video.name"
      :errors="err.specific.name"
    )
    c-form-select(
      title="Visibility"
      v-model.number="video.visibility"
      :errors="err.specific.visibility"
    )
      c-form-option(title="Public" :value="Visibility.Public")
      c-form-option(title="Unlisted" :value="Visibility.Unlisted")
      c-form-option(title="Private" :value="Visibility.Private")
    c-form-textarea(
      title="Description"
      placeholder="Provide additional information about your video."
      v-model="video.description"
      :errors="err.specific.description"
    )
    c-button.self-end Save
</template>
