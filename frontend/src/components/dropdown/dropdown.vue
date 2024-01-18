<script setup lang="ts">
import { ref } from "vue";

interface Props {
  align?: "left" | "right";
}

withDefaults(defineProps<Props>(), {
  align: "left",
});

const toggled = ref(false);

let clickedInside = true;
function mouseDown() {
  clickedInside = false;
}
function lostFocus() {
  if (clickedInside) {
    toggled.value = false;
  }
}
function click() {
  if (!(toggled.value && clickedInside)) {
    toggled.value = !toggled.value;
  }

  clickedInside = true;
}
</script>

<template lang="pug">
.relative.min-w-0(@click="click" @focusout="lostFocus" @mousedown="mouseDown")
  slot(name="button" :toggled="toggled")
  ul.absolute.min-w-fit.w-full.z-50(
    :class="{ 'right-0': align === 'right' }"
    v-show="toggled"
  )
    slot
</template>
