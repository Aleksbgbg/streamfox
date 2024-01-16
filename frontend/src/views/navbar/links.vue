<script setup lang="ts">
defineProps<{
  pages: string[];
}>();
</script>

<template lang="pug">
ul.flex.gap-3
  li.capitalize(v-for="page of pages" :key="page")
    router-link.text-xl.text-neutral-400.py-5.px-2(
      class="[&.router-link-exact-active]:text-white [&.router-link-exact-active]:font-bold"
      :to="{ name: page }"
    ) {{ page }}
</template>
