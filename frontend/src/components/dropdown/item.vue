<script setup lang="ts">
import CAnchorRoot from "@/components/anchor/root.vue";
</script>

<template lang="pug">
c-anchor-root
  li.bg-aurora-purple.truncate.max-w-full.px-3.py-2.cursor-pointer(
    class="hover:bg-aurora-purple-400"
  )
    slot
</template>
