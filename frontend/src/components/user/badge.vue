<script setup lang="ts">
import CUserAvatar from "@/components/user/avatar.vue";
import type { User } from "@/endpoints/user";

defineProps<{
  user: User;
}>();
</script>

<template lang="pug">
.flex.truncate
  c-user-avatar.w-6(:user="user")
  span.truncate.ml-2 {{ user.username }}
</template>
