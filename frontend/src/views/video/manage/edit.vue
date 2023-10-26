<script setup lang="ts">
import { type Ref, onBeforeMount, reactive, ref } from "vue";
import { useRoute } from "vue-router";
import CFormInput from "@/components/forms/input.vue";
import CFormOption from "@/components/forms/select/option.vue";
import CFormSelect from "@/components/forms/select/select.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import CFormLayout from "@/components/layout/form.vue";
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
c-form-layout(title="Edit Video")
  form.flex.justify-center.gap-6.mx-5.my-4(@submit.prevent="save")
    .flex.flex-col.items-center.gap-3
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
        title="Description" v-model="video.description"
        placeholder="Provide additional information about your video. "
        :errors="err.specific.description"
      )
      div(v-if="err.generic.length > 0")
        p.text-aurora-red.text-center(v-for="error of err.generic") {{ error }}
      p.text-aurora-green.text-center(v-if="updateComplete") Saved!
      button.self-end(
        class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2"
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
