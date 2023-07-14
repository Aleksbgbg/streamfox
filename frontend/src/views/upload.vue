<script setup lang="ts">
import { type Ref, computed, reactive, ref } from "vue";
import { uploadProgressReporter } from "@/bootstrapper/upload-progress-reporter";
import CForm from "@/components/forms/form.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormSelect from "@/components/forms/select.vue";
import CFormTextarea from "@/components/forms/textarea.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import {
  type VideoCreatedInfo,
  Visibility,
  createVideo,
  updateVideo,
  uploadVideo,
  videoThumbnail,
} from "@/endpoints/video";
import type { GenericErrors, SpecificErrors } from "@/types/errors";
import { checkAxiosError } from "@/utils/axiosError";
import { panic } from "@/utils/panic";

const fileInput: Ref<HTMLInputElement | null> = ref(null);

const createErrors: GenericErrors = reactive([]);
const uploadErrors: GenericErrors = reactive([]);

const updateErrors: GenericErrors = reactive([]);
const specificErrors: SpecificErrors = reactive({
  name: [],
  description: [],
  visibility: [],
});

const upload = reactive({
  inProgress: false,
  complete: false,
  saved: false,
  progressPercentage: 0,
  dataRate: 0,
});
const video: Ref<VideoCreatedInfo> = ref({
  id: "",
  name: "",
  description: "",
  visibility: Visibility.Public,
});
const meta = reactive({
  filename: "",
  thumbnail: "",
});

const roundedDataRate = computed(() => (Math.round(upload.dataRate * 100) / 100).toFixed(2));

async function fileSelected() {
  createErrors.length = 0;
  uploadErrors.length = 0;

  try {
    video.value = (await createVideo()).data;
  } catch (e) {
    const err = checkAxiosError<{ errors: string }>(e);
    createErrors.push(err.response?.data.errors || err.message);
    return;
  }

  const file: File = fileInput.value?.files?.[0] ?? panic("no file found");
  meta.filename = file.name;
  const reader = new FileReader();
  reader.onloadend = async function () {
    try {
      upload.inProgress = true;

      await uploadVideo(
        video.value.id,
        reader.result as ArrayBuffer,
        uploadProgressReporter.createProgressReporter((progressReport) => {
          upload.progressPercentage = progressReport.uploadedFraction * 100;
          upload.dataRate = progressReport.dataRateBytesPerSecond / 1000000;
        })
      );

      upload.inProgress = false;
      upload.complete = true;

      meta.thumbnail = videoThumbnail(video.value.id);
    } catch (e) {
      upload.inProgress = false;

      const err = checkAxiosError<{ errors: string }>(e);
      uploadErrors.push(err.response?.data.errors || err.message);
      return;
    }
  };
  reader.readAsArrayBuffer(file);
}

async function save() {
  updateErrors.length = 0;
  for (const key of Object.keys(specificErrors)) {
    specificErrors[key].length = 0;
  }

  try {
    await updateVideo(video.value.id, {
      name: video.value.name,
      visibility: video.value.visibility,
      description: video.value.description,
    });

    upload.saved = true;
    setTimeout(() => {
      upload.saved = false;
    }, 300);
  } catch (e) {
    const err = checkAxiosError<{ errors: SpecificErrors }>(e);
    const responseErrors = err.response?.data.errors;

    if (responseErrors === undefined) {
      uploadErrors.push("Log in failed for an unknown reason.");
    } else if (typeof responseErrors === "object") {
      for (const key of Object.keys(specificErrors)) {
        if (responseErrors[key]) {
          specificErrors[key] = responseErrors[key];
        }
      }
    } else {
      uploadErrors.push(responseErrors);
    }
    return;
  }
}
</script>

<template lang="pug">
c-center-main
  c-form(title="Upload Video" @submit="save")
    .flex.flex-col.items-center(v-if="!upload.complete")
      input(
        v-show="!upload.inProgress"
        class="file:border-0 file:rounded-full file:font-semibold file:bg-snow-dark file:text-frost-deep hover:file:bg-snow-lightest hover:file:cursor-pointer file:py-2 file:px-4 file:mr-3"
        type="file" ref="fileInput" @change="fileSelected"
      )
      p.text-aurora-red.mt-2(v-if="createErrors.length > 0" v-for="error of createErrors") {{ error }}
      div(v-if="upload.inProgress")
        span.block.text-center Uploading...
        span.block.text-center {{ roundedDataRate }} MB/s
        .bg-white.h-2.w-64
          .bg-theme-darkest.h-2(:style="{ width: `${upload.progressPercentage}%` }")
      .text-aurora-red.mt-2(v-if="uploadErrors.length > 0")
        p Your upload failed. The most common reasons for this are unsupported file formats and videos bigger than 100MB.
        p(v-for="error of uploadErrors") {{ error }}
    .flex.gap-6.mx-10.my-2(v-else)
      div
        .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
          c-form-input(
            title="Name" v-model="video.name"
            placeholder="Describe your video."
            :errors="specificErrors.name"
          )
          c-form-select(
            title="Visibility" v-model="video.visibility"
            :errors="specificErrors.visibility"
          )
            option(:value="Visibility.Public") Public
            option(:value="Visibility.Unlisted") Unlisted
            option(:value="Visibility.Private") Private
          c-form-textarea(
            title="Description" v-model="video.description"
            placeholder="Provide additional information about your video. "
            :errors="specificErrors.description"
          )
        .mb-5(v-if="updateErrors.length > 0")
          p.text-aurora-red(v-for="error of updateErrors") {{ error }}
        p.text-aurora-green.text-center.mb-5(v-if="upload.saved") Saved!
        button(
          class="float-right bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2"
          type="submit"
        ) Save
      .col-start-3.bg-polar-darkest.drop-shadow-2xl
        img.rounded-t-md.w-full(:src="meta.thumbnail")
        .p-4
          .mb-2
            label.font-semibold(for="video-link") Video link
            router-link.block.underline.text-theme-lightest(
              id="video-link" :to="{ name: 'watch', params: { id: video.id } }"
            ) watch/{{ video.id }}
          label.font-semibold(for="file") File
          p(id="file") {{ meta.filename }}
</template>
