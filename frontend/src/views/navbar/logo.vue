<script setup lang="ts">
import logo from "@/assets/logo.svg?component";
</script>

<template lang="pug">
router-link.flex.items-center(:to="{ name: 'home' }")
  logo.fill-white.w-14.-mt-4
  h1.font-bold.text-2xl.-ml-4 Streamfox
</template>
