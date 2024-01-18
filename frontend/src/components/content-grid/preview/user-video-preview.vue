<script setup lang="ts">
import { computed } from "vue";
import CAnchorCover from "@/components/anchor/cover.vue";
import CButton from "@/components/button.vue";
import CPreviewGeneric from "@/components/content-grid/preview/generic.vue";
import CDropdown from "@/components/dropdown/dropdown.vue";
import CDropdownItem from "@/components/dropdown/item.vue";
import CIcon from "@/components/icon.vue";
import { type VideoInfo, Visibility, videoThumbnail } from "@/endpoints/video";

const props = defineProps<{
  isPersonalPage: boolean;
  video: VideoInfo;
}>();

const icon = computed(() => {
  switch (props.video.visibility) {
    case Visibility.Private:
      return "lock-fill";
    case Visibility.Unlisted:
      return "eye-slash-fill";
    case Visibility.Public:
      return "eye-fill";
    default:
      return "";
  }
});
</script>

<template lang="pug">
c-preview-generic(
  :link="{ name: 'watch', params: { id: video.id } }"
  :thumbnailUrl="videoThumbnail(video.id)"
  :durationSecs="video.durationSecs"
  :name="video.name"
  :creator="video.creator"
  :beginAt="video.uploadedAt"
  :viewership="video.views"
  viewershipName="view"
)
  .flex.flex-col.justify-between.items-center(v-if="isPersonalPage")
    c-dropdown(align="right")
      c-dropdown-item
        router-link(:to="{ name: 'edit', params: { videoId: video.id } }")
          span Edit video
          c-anchor-cover
      template(#button="{ toggled }")
        c-button.text-xl(
          :class="{ 'rounded-b-none': toggled }"
          padding="small"
          :theme="toggled ? 'blue' : 'invisible'"
        )
          c-icon(name="three-dots-vertical")
    c-icon.text-slate-300(:name="icon")
</template>
