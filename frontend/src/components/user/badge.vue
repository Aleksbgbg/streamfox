<script setup lang="ts">
import CUserAvatar from "@/components/user/avatar.vue";
import type { User } from "@/endpoints/user";

defineProps<{
  user: User;
}>();
</script>

<template lang="pug">
.flex.truncate
  c-user-avatar(:user="user" size="25px")
  span.truncate.ml-2 {{ user.username }}
</template>
