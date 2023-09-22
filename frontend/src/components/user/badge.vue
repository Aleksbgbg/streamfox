<script setup lang="ts">
import CUserAvatar from "@/components/user/avatar.vue";
import type { User } from "@/endpoints/user";

const props = defineProps<{
  user: User;
}>();
</script>

<template lang="pug">
.flex
  c-user-avatar(class="w-[25px] h-[25px]" :user="user")
  span.ml-2 {{ user.username }}
</template>
