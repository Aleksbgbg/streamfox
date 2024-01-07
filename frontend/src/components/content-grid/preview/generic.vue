<script setup lang="ts">
import { computed, defineProps, withDefaults } from "vue";
import type { RouteLocationNamedRaw, RouteLocationRaw } from "vue-router";
import CAnchorCover from "@/components/anchor/cover.vue";
import CAnchorRoot from "@/components/anchor/root.vue";
import CButton from "@/components/button.vue";
import CDropdown from "@/components/dropdown/dropdown.vue";
import CDropdownItem from "@/components/dropdown/item.vue";
import CIcon from "@/components/icon.vue";
import type { User } from "@/endpoints/user";
import { dateToElapsedTimeString, secsToDurationString } from "@/utils/strings";

const props = withDefaults(
  defineProps<{
    link: RouteLocationRaw;
    thumbnailUrl: string;
    durationSecs: number;
    name: string;
    creator: User;
    beginAt: Date;
    viewership: number;
    viewershipName: string;
    icon?: string;
    kebabMenu?: boolean;
  }>(),
  { icon: undefined, kebabMenu: false },
);

const videoId = computed(() => {
  return `${(props.link as RouteLocationNamedRaw)?.params?.id?.toString() ?? props.link}`;
});
</script>

<template lang="pug">
c-anchor-root.group.w-full(class="max-w-[416px]")
  .relative.aspect-video
    img(src="@/assets/fox.png" alt="")
    img.absolute.top-0(:src="thumbnailUrl" alt="")
    span(
      class="absolute bottom-0.5 right-0.5 text-sm opacity-85 bg-black px-1"
    ) {{ secsToDurationString(durationSecs) }}
  .flex.justify-between.gap-2.mt-2
    .min-w-0
      router-link(:to="link")
        h3.break-words.line-clamp-2.font-semibold(
          class="group-hover:underline group-hover:text-aurora-yellow"
        ) {{ name }}
        c-anchor-cover
      p.text-sm {{ creator.username }}
      p.text-sm
        | {{ viewership }} {{ viewershipName + (viewership === 1 ? '' : 's') }}
        | •
        | {{ dateToElapsedTimeString(beginAt) }}
    .flex.flex-col.justify-between.items-center
      template(v-if="kebabMenu")
        c-dropdown.flex.justify-center(class="[&_>_ul]:w-fit [&_>_ul]:right-0 [&_>_ul]:top-full")
          c-dropdown-item(class="[&_>_li]:!bg-frost-blue hover:[&_>_li]:!bg-frost-deep")
            router-link.block(:to="{ name: 'edit', params: { videoId: videoId } }")
              span Edit video
              c-anchor-cover

          template(#button="{ toggled }")
            c-button.text-xl(class="!p-0 !px-2" :theme="toggled ? 'blue' : 'invisible'") ⋮
      c-icon.text-slate-300(v-if="icon" :name="icon")
</template>
