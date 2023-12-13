<script setup lang="ts">
interface Props {
  collection: unknown[];
  empty: string;
  textMargin?: boolean;
}

withDefaults(defineProps<Props>(), {
  textMargin: false,
});
</script>

<template lang="pug">
template(v-if="collection.length > 0")
  slot
template(v-else)
  p.text-aurora-yellow-300.text-center(:class="{ 'm-5': textMargin }") {{ empty }}
</template>
