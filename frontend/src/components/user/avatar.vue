<script setup lang="ts">
import type { User } from "@/endpoints/user";
import { newRng } from "@/utils/rng";

const props = defineProps<{
  user: User;
}>();

const rng = newRng(props.user.id);

const h = Math.round(rng(0, 360));
const s = Math.round(rng(25, 60));
const l = Math.round(rng(25, 50));
</script>

<template lang="pug">
div(class="rounded-full min-w-[25px] min-h-[25px]" :style="`background-color: hsl(${h}, ${s}%, ${l}%)`")
  span.inline-block.font-bold.text-center.w-full {{ user.username.charAt(0) }}
</template>
