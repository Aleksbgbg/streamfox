<script setup lang="ts">
import CNavbarCompact from "@/views/navbar/compact.vue";
import CNavbarExpansive from "@/views/navbar/expansive.vue";

const pages = ["home", "upload", "live"];
</script>

<template lang="pug">
.p-5
  div(class="md:hidden")
    c-navbar-compact(:pages="pages")
  div(class="hidden md:block")
    c-navbar-expansive(:pages="pages")
</template>
