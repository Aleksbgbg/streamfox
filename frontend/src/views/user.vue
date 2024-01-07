<script setup lang="ts">
import { type Ref, computed, onBeforeMount, ref, watch } from "vue";
import CUserVideoPreview from "@/components/content-grid/preview/user-video-preview.vue";
import CTileGrid from "@/components/content-grid/tile-grid.vue";
import CEmptyCollection from "@/components/empty-collection.vue";
import { useToaster } from "@/components/toasts/toaster";
import CUserBadge from "@/components/user/badge.vue";
import { type User, type UserId, getUserById } from "@/endpoints/user";
import { type VideoInfo, getUserVideos } from "@/endpoints/video";
import { useUserStore } from "@/store/user";
import { type Optional, empty, getValue, hasValue, mapOrElse } from "@/types/optional";

const toaster = useToaster();

const userStore = useUserStore();

const props = defineProps<{
  userId: UserId;
}>();

const user: Ref<Optional<User>> = ref(empty());
const videos: Ref<VideoInfo[]> = ref([]);

const isPersonalPage = computed(
  () =>
    hasValue(userStore.user) &&
    hasValue(user.value) &&
    getValue(userStore.user).id === getValue(user.value).id,
);

async function fetchUser() {
  const [userResponse, videosResponse] = await Promise.all([
    getUserById(props.userId),
    getUserVideos(props.userId),
  ]);

  if (!userResponse.success()) {
    toaster.failure("Unable to fetch user.");
    return;
  }

  if (!videosResponse.success()) {
    toaster.failure("Unable to fetch videos.");
    return;
  }

  user.value = userResponse.value();
  videos.value = videosResponse.value();
}
watch(() => props.userId, fetchUser);
onBeforeMount(fetchUser);
</script>

<template lang="pug">
.rounded.bg-aurora-purple.w-min.max-w-full.mx-auto.my-5.p-5(v-if="user")
  c-user-badge(:user="user")
c-empty-collection(
  :collection="videos"
  :empty="`${mapOrElse(user, (user) => user.username, 'The user')} has not uploaded any videos yet. 😢`"
  text-margin
)
  c-tile-grid
    c-user-video-preview(v-for="video of videos" :show-visibility="isPersonalPage" :video="video" kebabMenu)
</template>
