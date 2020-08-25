<template lang="pug">
div(@loaded="loaded")
  form.hidden
    input(type="file" ref="fileInput" @change="fileSelected")
  .flex.justify-center
    .flex.flex-col.items-center.bg-theme-light.rounded-lg.p-5.m-5
      h2.font-bold.text-lg Video Upload
      div(v-show="upload.started")
        span.block.text-center Uploading...
        span.block.text-center {{ roundedDataRate }} MB/s
        .bg-white.h-2.w-64
          .bg-theme-darkest.h-2(:style="{ width: `${upload.progressBarPercentage}%` }")
      .flex.flex-col.items-center(v-show="video.isUploaded")
        span.block.text-center Your video has uploaded.
        router-link.my-3(:to="{ name: 'watch', params: { id: video.id }}")
          img(:src="video.url" alt="")
        p
          | Visit your video at
          |
          router-link.underline.text-theme-lightest(:to="{ name: 'watch', params: { id: video.id }}") watch/{{ video.id }}
          | .
      div(v-show="upload.failed")
        p Your upload failed. The most common reasons for this are unsupported file formats and videos bigger than 100MB.
</template>

<script>
import { videoUploader } from "@/bootstrapper/video-endpoint";
import { uploadProgressReporter } from "@/bootstrapper/upload-progress-reporter";
import { endpointResolver } from "@/bootstrapper/endpoint-resolver";

export default {
  data() {
    return {
      upload: {
        started: false,
        failed: false,
        progressBarPercentage: 0,
        dataRate: 0
      },
      video: {
        isUploaded: false,
        id: 0,
        url: null
      }
    };
  },
  mounted() {
    this.loaded();
  },
  computed: {
    roundedDataRate() {
      return (Math.round(this.upload.dataRate * 100) / 100).toFixed(2);
    }
  },
  methods: {
    loaded() {
      this.$refs.fileInput.click();
    },
    fileSelected() {
      this.upload.started = true;

      const file = this.$refs.fileInput.files[0];
      const reader = new FileReader();

      reader.onloadend = async() => {
        try {
          const response = await videoUploader.uploadVideo(reader.result, uploadProgressReporter.createProgressReporter((progressReport) => {
            this.upload.progressBarPercentage = progressReport.uploadedFraction * 100;
            this.upload.dataRate = progressReport.dataRateBytesPerSecond / 1000000;
          }));
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
      this.video.url = endpointResolver.resolve(`/videos/${response.videoId}/thumbnail`);
    },
    onFailure() {
      this.upload.started = false;
      this.upload.failed = true;
    }
  }
};
</script>