<script setup lang="ts">
import CNavButtons from "@/views/navbar/buttons.vue";
import CNavLinks from "@/views/navbar/links.vue";
import CLogo from "@/views/navbar/logo.vue";

defineProps<{
  pages: string[];
}>();
</script>

<template lang="pug">
nav.flex.items-center.justify-between
  c-logo
  c-nav-links.mx-5(:pages="pages")
  c-nav-buttons.min-w-0
</template>
