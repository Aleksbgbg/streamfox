<script setup lang="ts">
import CUploadButton from "@/components/upload-button.vue";
import { useUserStore } from "@/store/user";
import { hasValue } from "@/utils/optional";

const store = useUserStore();
</script>

<template lang="pug">
nav.flex.h-20.p-5
  router-link(:to="{ name: 'home' }")
    h1.font-bold.text-2xl Streamfox
  c-upload-button.flex-grow
  .self-center.flex(v-if="hasValue(store.user)")
    i.bi-question-square
    p.ml-2 {{ store.user.username }}
  .self-center(v-else)
    router-link.rounded.hover_bg-polar-light.px-3.py-2(:to="{ name: 'login' }")
      i.bi-box-arrow-in-right
      span Log In
    router-link.rounded.hover_bg-polar-light.px-3.py-2.ml-2(:to="{ name: 'register' }")
      i.bi-person
      span Register
</template>
