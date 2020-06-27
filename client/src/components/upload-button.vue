<template lang="pug">
div
  form.hidden
    input(type="file" ref="fileInput" @change="fileSelected")
  button.rounded.font-bold.border-b-4.bg-blue-500.border-blue-700.hover-bg-blue-700.hover-border-blue-900.transition.duration-500.ease-in-out.transform.hover--translate-y-1.hover-scale-110.mx-5.py-2.px-4(@click="triggerUpload") Upload Video
</template>

<script>
import { videoUploader } from "@/bootstrapper/video-endpoint";

export default {
  methods: {
    triggerUpload() {
      this.$refs.fileInput.click();
    },
    fileSelected() {
      this.$emit("uploadStart");

      const file = this.$refs.fileInput.files[0];
      const reader = new FileReader();

      reader.onloadend = async() => {
        try {
          const response = await videoUploader.uploadVideo(reader.result);
          this.$emit("uploadSuccess", response.videoId);
        } catch {
          this.$emit("uploadFail");
        }
      };

      reader.readAsArrayBuffer(file);
    }
  }
};
</script>