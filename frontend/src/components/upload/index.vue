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

<script>
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import { uploadProgressReporter } from "@/bootstrapper/upload-progress-reporter";
import { videoProgressFetcher, videoUploader } from "@/bootstrapper/video-endpoint";

function formatTime(totalSeconds) {
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

export default {
  data() {
    return {
      upload: {
        startedOnce: false,
        started: false,
        failed: false,
        progressPercentage: 0,
        dataRate: 0,
      },
      conversion: {
        started: false,
        progressPercentage: 0,
        frameCurrent: 0,
        framesTotal: 0,
        timeCurrent: 0,
        timeRemaining: 0,
      },
      video: {
        isUploaded: false,
        isDone: false,
        id: 0,
        thumbnailUrl: null,
      },
    };
  },
  computed: {
    roundedDataRate() {
      return (Math.round(this.upload.dataRate * 100) / 100).toFixed(2);
    },
    timeElapsed() {
      return formatTime(this.conversion.timeCurrent);
    },
    timeRemaining() {
      return formatTime(this.conversion.timeRemaining);
    },
  },
  methods: {
    fileSelected() {
      this.upload.startedOnce = true;
      this.upload.started = true;

      const file = this.$refs.fileInput.files[0];
      const reader = new FileReader();

      reader.onloadend = async () => {
        try {
          const response = await videoUploader.uploadVideo(
            reader.result,
            uploadProgressReporter.createProgressReporter((progressReport) => {
              this.upload.progressPercentage = progressReport.uploadedFraction * 100;
              this.upload.dataRate = progressReport.dataRateBytesPerSecond / 1000000;
            })
          );
          this.onSuccess(response);
        } catch {
          this.onFailure();
        }
      };

      reader.readAsArrayBuffer(file);
    },
    onSuccess(response) {
      this.upload.started = false;
      this.video.isUploaded = true;
      this.video.id = response.videoId;
      this.video.thumbnailUrl = endpointResolver.resolve(`/videos/${response.videoId}/thumbnail`);

      this.beginConversion();
    },
    onFailure() {
      this.upload.started = false;
      this.upload.failed = true;
    },
    beginConversion() {
      setTimeout(this.startProgressLookupLoop.bind(this), 1000);
    },
    async startProgressLookupLoop() {
      let progress;

      try {
        progress = await videoProgressFetcher.fetchProgress(this.video.id);
      } catch {
        this.conversion.progressPercentage = 100;
        this.conversion.frameCurrent = this.conversion.framesTotal;
        this.conversion.timeRemaining = 0;

        this.video.isDone = true;

        return;
      }

      this.conversion.started = true;

      this.conversion.progressPercentage = progress.doneFraction * 100;
      this.conversion.frameCurrent = progress.currentFrame;
      this.conversion.framesTotal = progress.videoDuration;
      this.conversion.timeCurrent = progress.timeElapsed;
      this.conversion.timeRemaining = progress.timeRemaining;

      setTimeout(this.startProgressLookupLoop.bind(this), 1000);
    },
  },
};
</script>
