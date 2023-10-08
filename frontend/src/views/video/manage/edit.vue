<script setup lang="ts">
import { type Ref, onBeforeMount, reactive, ref } from "vue";
import { useRoute } from "vue-router";
import CForm from "@/components/forms/form.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormSelect from "@/components/forms/select.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import {
  type VideoId,
  type VideoUpdateInfo,
  Visibility,
  getVideoInfo,
  updateVideo,
  videoThumbnail,
} from "@/endpoints/video";

const props = defineProps<{
  videoId: VideoId;
}>();

const route = useRoute();

const video: VideoUpdateInfo = reactive({
  name: "",
  description: "",
  visibility: Visibility.Public,
});

const updateComplete = ref(false);
const err: Ref<ApiErr<VideoUpdateInfo>> = ref(emptyApiErr());

onBeforeMount(async () => {
  const response = await getVideoInfo(props.videoId);

  if (!response.success()) {
    err.value.generic = response.err().generic;
    return;
  }

  const value = response.value();
  video.name = value.name;
  video.description = value.description;
  video.visibility = value.visibility;
});

async function save() {
  err.value = emptyApiErr();

  const response = await updateVideo(props.videoId, video);

  if (!response.success()) {
    err.value = response.err();
    return;
  }

  updateComplete.value = true;
  setTimeout(() => (updateComplete.value = false), 1000);
}
</script>

<template lang="pug">
c-center-main
  c-form(title="Edit Video" @submit="save")
    .flex.gap-6.mx-10.my-2
      div
        .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
          c-form-input(
            title="Name" v-model="video.name"
            placeholder="Describe your video."
            :errors="err.specific.name"
          )
          c-form-select(
            title="Visibility" v-model.number="video.visibility"
            :errors="err.specific.visibility"
          )
            option(:value="Visibility.Public") Public
            option(:value="Visibility.Unlisted") Unlisted
            option(:value="Visibility.Private") Private
          c-form-textarea(
            title="Description" v-model="video.description"
            placeholder="Provide additional information about your video. "
            :errors="err.specific.description"
          )
        .text-center.mb-5(v-if="err.generic.length > 0")
          p.text-aurora-red(v-for="error of err.generic") {{ error }}
        p.text-aurora-green.text-center.mb-5(v-if="updateComplete") Saved!
        button(
          class="float-right bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2"
          type="submit"
        ) Save
      .col-start-3.bg-polar-darkest.drop-shadow-2xl
        img.rounded-t-md.w-full(:src="videoThumbnail(props.videoId)")
        .p-4
          .mb-2
            label.font-semibold(for="video-link") Video link
            router-link.block.underline.text-frost-cyan(
              id="video-link" :to="{ name: 'watch', params: { id: videoId } }"
            ) watch/{{ videoId }}
          div(v-if="route.query.filename")
            label.font-semibold(for="file") File
            p(id="file") {{ route.query.filename }}
</template>
