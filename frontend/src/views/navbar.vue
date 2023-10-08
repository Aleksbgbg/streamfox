<script setup lang="ts">
import logo from "@/assets/logo.svg?component";
import CUploadButton from "@/components/upload-button.vue";
import CUserBadge from "@/components/user/badge.vue";
import { useUserStore } from "@/store/user";
import { getValue, hasValue } from "@/types/optional";
import { logout } from "@/utils/auth";

const store = useUserStore();
</script>

<template lang="pug">
nav.flex.items-center.p-5
  router-link.flex.items-center(:to="{ name: 'home' }")
    logo.fill-white.w-14.-mt-4
    h1.font-bold.text-2xl.-ml-4 Streamfox
  .flex-grow
    c-upload-button
  .flex.items-center(v-if="hasValue(store.user)")
    c-user-badge(:user="getValue(store.user)")
    button(
      class="hover:bg-polar-light rounded px-3 py-2"
      @click="logout"
    )
      span Log Out
      i.bi-box-arrow-in-right
  div(v-else)
    router-link(
      class="hover:bg-polar-light rounded px-3 py-2 ml-2"
      :to="{ name: 'login' }"
    )
      i.bi-box-arrow-in-right
      span Log In
    router-link(
      class="hover:bg-polar-light rounded px-3 py-2 ml-2"
      :to="{ name: 'register' }"
    )
      i.bi-person
      span Register
</template>
