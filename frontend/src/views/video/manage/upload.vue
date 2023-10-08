<script setup lang="ts">
import { type Ref, computed, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import CForm from "@/components/forms/form.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { type VideoCreatedInfo, Visibility, createVideo, uploadVideo } from "@/endpoints/video";
import { panic } from "@/utils/panic";

const router = useRouter();

const fileInput: Ref<HTMLInputElement | null> = ref(null);

const createErr: Ref<ApiErr<VideoCreatedInfo>> = ref(emptyApiErr());
const uploadErr: Ref<ApiErr<void>> = ref(emptyApiErr());

const upload = reactive({
  created: false,
  inProgress: false,
  progressPercentage: 0,
  dataRate: 0,
});
const video: Ref<VideoCreatedInfo> = ref({
  id: "",
  name: "",
  description: "",
  visibility: Visibility.Public,
});

const roundedDataRate = computed(() => (Math.round(upload.dataRate * 100) / 100).toFixed(2));

function removeExtension(filename: string): string {
  return filename.replace(/\.[^/.]+$/, "");
}

async function fileSelected() {
  createErr.value = emptyApiErr();
  uploadErr.value = emptyApiErr();

  const file = fileInput.value?.files?.[0] ?? panic("no file found");

  if (!upload.created) {
    const createResponse = await createVideo({ name: removeExtension(file.name) });

    if (!createResponse.success()) {
      createErr.value = createResponse.err();
      return;
    }

    video.value = createResponse.value();
    upload.created = true;
  }

  const reader = new FileReader();
  reader.onloadend = async function () {
    upload.inProgress = true;
    const uploadResponse = await uploadVideo(
      video.value.id,
      reader.result as ArrayBuffer,
      function (progressReport) {
        upload.progressPercentage = progressReport.uploadedFraction * 100;
        upload.dataRate = progressReport.dataRateBytesPerSec / 1000000;
      }
    );
    upload.inProgress = false;

    if (!uploadResponse.success()) {
      uploadErr.value = uploadResponse.err();
      return;
    }

    router.push({
      name: "edit",
      params: { videoId: video.value.id },
      query: { filename: file.name },
    });
  };
  reader.readAsArrayBuffer(file);
}
</script>

<template lang="pug">
c-center-main
  c-form(title="Upload Video")
    .flex.flex-col.items-center
      input(
        v-show="!upload.inProgress"
        class="file:border-0 file:rounded-full file:font-semibold file:bg-snow-dark file:text-frost-deep hover:file:bg-snow-lightest hover:file:cursor-pointer file:py-2 file:px-4 file:mr-3"
        type="file" ref="fileInput" @change="fileSelected"
      )
      p.text-aurora-red.mt-2(
        v-if="createErr.generic.length > 0"
        v-for="error of createErr.generic"
      ) {{ error }}
      div(v-if="upload.inProgress")
        span.block.text-center Uploading...
        span.block.text-center {{ roundedDataRate }} MB/s
        .bg-white.h-2.w-64
          .bg-theme-darkest.h-2(:style="{ width: `${upload.progressPercentage}%` }")
      .text-aurora-red.text-center.mt-2(v-if="uploadErr.generic.length > 0")
        p Your upload failed. Common reasons for this are unsupported file formats and videos bigger than 100MB.
        p See below:
        p(v-for="error of uploadErr.generic") {{ error }}
</template>
