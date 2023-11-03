<script setup lang="ts">
import { type Ref, computed, onUpdated, ref } from "vue";
import { type Point } from "@/types/point";
import { fracToPercent } from "@/utils/math";

const show = ref(false);
const location = ref({ x: 0, y: 0 });

const menu: Ref<HTMLDivElement | null> = ref(null);

const locationPercent = computed(() => ({
  x: fracToPercent(location.value.x / window.innerWidth),
  y: fracToPercent(location.value.y / window.innerHeight),
}));

let wantsFocus = false;
function showMenu(loc: Point) {
  show.value = true;
  location.value = loc;
  wantsFocus = true;
}
function hideMenu() {
  show.value = false;
}
onUpdated(() => {
  if (wantsFocus) {
    menu.value?.focus();
    wantsFocus = false;
  }
});

defineExpose({
  show: showMenu,
});
</script>

<template lang="pug">
.fixed.flex.flex-col.bg-frost-deep.opacity-95.overflow-clip.py-2(
  class="w-[300px]"
  v-show="show"
  ref="menu"
  tabindex="0"
  :style="{ left: `min(${locationPercent.x}vw, 100vw - 300px)`, top: `${locationPercent.y}vh` }"
  @click="hideMenu"
  @focusout="hideMenu"
)
  slot
</template>
