<script setup lang="ts">
import { computed } from "vue";
import type { User } from "@/endpoints/user";
import { newRng } from "@/utils/rng";

const props = defineProps<{
  user: User;
}>();

const colour = computed(() => {
  const rng = newRng(props.user.id);

  const h = Math.round(rng(0, 360));
  const s = Math.round(rng(25, 60));
  const l = Math.round(rng(25, 50));

  return { h, s, l };
});
</script>

<template lang="pug">
.flex.items-center.justify-center.rounded-full.aspect-square.select-none(
  :style="{ 'background-color': `hsl(${colour.h}, ${colour.s}%, ${colour.l}%)`, 'container-type': 'inline-size' }"
)
  span.font-bold.uppercase(:style="{ 'font-size': '60cqw' }") {{ user.username.charAt(0) }}
</template>
