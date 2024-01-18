<script setup lang="ts">
import CAnchorRoot from "@/components/anchor/root.vue";
</script>

<template lang="pug">
c-anchor-root
  li.bg-frost-blue.truncate.max-w-full.px-3.py-2.cursor-pointer(
    class="hover:bg-frost-deep"
  )
    slot
</template>
