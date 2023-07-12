<script setup lang="ts">
import { type Ref, computed, reactive, ref } from "vue";
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import { uploadProgressReporter } from "@/bootstrapper/upload-progress-reporter";
import { videoProgressFetcher, videoUploader } from "@/bootstrapper/video-endpoint";
import { type VideoMetadata } from "@/endpoints/responses/video-metadata";
import { panic } from "@/utils/panic";

function formatTime(totalSeconds: number): string {
  if (totalSeconds > 86400) {
    return "more than 24 hours";
  }

  totalSeconds = Math.round(totalSeconds);

  let string = "";

  const minutes = Math.floor(totalSeconds / 60);

  if (minutes > 0) {
    string += `${minutes}:`;
  }

  const seconds = totalSeconds % 60;

  if (seconds < 10 && minutes !== 0) {
    string += "0";
  }

  string += `${seconds}`;

  if (minutes === 0) {
    string += "s";
  }

  return string;
}

const fileInput: Ref<HTMLInputElement | null> = ref(null);

const upload = reactive({
  startedOnce: false,
  started: false,
  failed: false,
  progressPercentage: 0,
  dataRate: 0,
});
const conversion = reactive({
  started: false,
  progressPercentage: 0,
  frameCurrent: 0,
  framesTotal: 0,
  timeCurrent: 0,
  timeRemaining: 0,
});
const video = reactive({
  isUploaded: false,
  isDone: false,
  id: "",
  thumbnailUrl: "",
});

const roundedDataRate = computed(() => (Math.round(upload.dataRate * 100) / 100).toFixed(2));
const timeElapsed = computed(() => formatTime(conversion.timeCurrent));
const timeRemaining = computed(() => formatTime(conversion.timeRemaining));

function fileSelected() {
  upload.startedOnce = true;
  upload.started = true;

  const file: File = fileInput.value?.files?.[0] ?? panic("no file found");
  const reader = new FileReader();
  reader.onloadend = async function () {
    try {
      const response = await videoUploader.uploadVideo(
        reader.result as ArrayBuffer,
        uploadProgressReporter.createProgressReporter((progressReport) => {
          upload.progressPercentage = progressReport.uploadedFraction * 100;
          upload.dataRate = progressReport.dataRateBytesPerSecond / 1000000;
        })
      );
      onSuccess(response);
    } catch {
      onFailure();
    }
  };
  reader.readAsArrayBuffer(file);
}

function onSuccess(response: VideoMetadata) {
  upload.started = false;
  video.isUploaded = true;
  video.id = response.videoId;
  video.thumbnailUrl = endpointResolver.resolve(`/videos/${response.videoId}/thumbnail`);

  beginConversion();
}

function onFailure() {
  upload.started = false;
  upload.failed = true;
}

function beginConversion() {
  setTimeout(startProgressLookupLoop, 1000);
}

async function startProgressLookupLoop() {
  let progress;

  try {
    progress = await videoProgressFetcher.fetchProgress(video.id);
  } catch {
    conversion.progressPercentage = 100;
    conversion.frameCurrent = conversion.framesTotal;
    conversion.timeRemaining = 0;

    video.isDone = true;

    return;
  }

  conversion.started = true;

  conversion.progressPercentage = progress.doneFraction * 100;
  conversion.frameCurrent = progress.currentFrame;
  conversion.framesTotal = progress.videoDuration;
  conversion.timeCurrent = progress.timeElapsed;
  conversion.timeRemaining = progress.timeRemaining;

  setTimeout(startProgressLookupLoop, 1000);
}
</script>

<template lang="pug">
div
  .flex.justify-center
    .flex.flex-col.items-center.bg-theme-light.rounded-lg.p-5.m-5
      h2.font-bold.text-lg Video Upload
      form(v-if="!upload.startedOnce")
        input(type="file" ref="fileInput" @change="fileSelected")
      div(v-show="upload.started")
        span.block.text-center Uploading...
        span.block.text-center {{ roundedDataRate }} MB/s
        .bg-white.h-2.w-64
          .bg-theme-darkest.h-2(:style="{ width: `${upload.progressPercentage}%` }")
      .flex.flex-col.items-center(v-if="video.isUploaded")
        span.block.text-center Your video has uploaded.
        router-link.my-3(:to="{ name: 'watch', params: { id: video.id }}")
          img(:src="video.thumbnailUrl" alt="")
      div(v-if="conversion.started && !video.isDone")
        span.block.text-center Your video is being converted to a supported format.
        span.block.text-center {{ conversion.frameCurrent }} / {{ conversion.framesTotal }} frames completed
        div.text-center
          .bg-white.h-2.w-64.inline-block
            .bg-theme-darkest.h-2(:style="{ width: `${conversion.progressPercentage}%` }")
        span.block.text-center {{ timeElapsed }} elapsed, {{ timeRemaining }} remaining
      div(v-if="video.isDone")
        p
          | Visit your video at
          |
          router-link.underline.text-theme-lightest(:to="{ name: 'watch', params: { id: video.id }}") watch/{{ video.id }}
          | .
      div(v-if="upload.failed")
        p Your upload failed. The most common reasons for this are unsupported file formats and videos bigger than 100MB.
</template>
