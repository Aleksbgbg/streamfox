<script setup lang="ts">
import { onBeforeMount, ref } from "vue";
import CUploadButton from "@/components/upload-button.vue";
import { getUser } from "@/endpoints/user";
import { empty, hasValue } from "@/utils/optional";

const user = ref(empty());

onBeforeMount(async () => (user.value = await getUser()));
</script>

<template lang="pug">
nav.flex.h-20.p-5
  router-link(:to="{ name: 'home' }")
    h1.font-bold.text-2xl Streamfox
  c-upload-button.flex-grow
  .self-center.flex(v-if="hasValue(user)")
    i.bi-question-square
    p.ml-2 {{ user.username }}
  .self-center(v-else)
    router-link.rounded.hover_bg-polar-light.px-3.py-2(:to="{ name: 'login' }")
      i.bi-box-arrow-in-right
      span Log In
    router-link.rounded.hover_bg-polar-light.px-3.py-2.ml-2(:to="{ name: 'register' }")
      i.bi-person
      span Register
</template>
