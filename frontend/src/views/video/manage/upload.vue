<script setup lang="ts">
import { type Ref, computed, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import CFormLayout from "@/components/layout/form.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type ApiErr, ApiResponse, emptyApiErr } from "@/endpoints/request";
import {
  type VideoCreatedInfo,
  Visibility,
  createVideo,
  updateVideo,
  uploadVideo,
} from "@/endpoints/video";
import { fracToPercent } from "@/utils/math";
import { panic } from "@/utils/panic";
import { createProgressReporter } from "@/utils/upload-progress";

const router = useRouter();

const toaster = useToaster();

const fileInput: Ref<HTMLInputElement | null> = ref(null);

const createErr: Ref<ApiErr<VideoCreatedInfo>> = ref(emptyApiErr());
const uploadErr: Ref<ApiErr<void>> = ref(emptyApiErr());

const upload = reactive({
  created: false,
  inProgress: false,
  progressPercentage: 0,
  dataRateMBs: 0,
});
const video: Ref<VideoCreatedInfo> = ref({
  id: "",
  name: "",
  description: "",
  visibility: Visibility.Public,
});

const roundedDataRateMBs = computed(() => (Math.round(upload.dataRateMBs * 100) / 100).toFixed(2));

async function uploadFile(file: File): Promise<ApiResponse<void, void>> {
  return new Promise(function (resolve) {
    const size = file.size;
    const chunkSizeBytes = 100_000_000;
    let readBytes = 0;

    const reportProgress = createProgressReporter(function (progressReport) {
      upload.progressPercentage = fracToPercent(progressReport.uploadedFraction);
      upload.dataRateMBs = progressReport.dataRateBytesPerSec / 1_000_000;
    });

    const reader = new FileReader();
    reader.onloadend = async function () {
      const uploadResponse = await uploadVideo(
        video.value.id,
        reader.result as ArrayBuffer,
        { start: readBytes, end: Math.min(readBytes + chunkSizeBytes, size) - 1, size },
        reportProgress,
      );

      if (!uploadResponse.success()) {
        resolve(uploadResponse);
        return;
      }

      readBytes += chunkSizeBytes;

      if (readBytes >= size) {
        resolve(uploadResponse);
        return;
      }

      reader.readAsArrayBuffer(file.slice(readBytes, readBytes + chunkSizeBytes));
    };
    reader.readAsArrayBuffer(file.slice(0, chunkSizeBytes));
  });
}

function removeExtension(filename: string): string {
  return filename.replace(/\.[^/.]+$/, "");
}

async function fileSelected() {
  createErr.value = emptyApiErr();
  uploadErr.value = emptyApiErr();

  const file = fileInput.value?.files?.[0] ?? panic("no file found");

  if (!upload.created) {
    const createResponse = await createVideo();

    if (!createResponse.success()) {
      createErr.value = createResponse.err();
      toaster.failureAll(createErr.value.generic);
      return;
    }

    video.value = createResponse.value();
    upload.created = true;
  }

  upload.inProgress = true;
  const uploadResponse = await uploadFile(file);

  if (!uploadResponse.success()) {
    uploadErr.value = uploadResponse.err();
    toaster.failureAll(uploadErr.value.generic);
    upload.inProgress = false;
    return;
  }

  await updateVideo(video.value.id, {
    name: removeExtension(file.name),
    description: video.value.description,
    visibility: video.value.visibility,
  });

  router.push({
    name: "edit",
    params: { videoId: video.value.id },
    query: { filename: file.name },
  });
}
</script>

<template lang="pug">
c-form-layout(title="Upload Video")
  .flex.flex-col.items-center.my-5
    input(
      v-show="!upload.inProgress"
      class="file:border-0 file:rounded-full file:font-semibold file:bg-snow-dark file:text-frost-deep hover:file:bg-snow-lightest hover:file:cursor-pointer file:py-2 file:px-4 file:mr-3"
      type="file" ref="fileInput" @change="fileSelected"
    )
    div(v-if="upload.inProgress")
      span.block.text-center Uploading...
      span.block.text-center {{ roundedDataRateMBs }} MB/s
      .bg-white.h-2.w-64
        .bg-frost-blue.h-2(:style="{ width: `${upload.progressPercentage}%` }")
</template>
