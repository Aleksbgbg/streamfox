<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import CButton from "@/components/button.vue";
import CIcon from "@/components/icon.vue";
import CNavButtons from "@/views/navbar/buttons.vue";
import CNavLinks from "@/views/navbar/links.vue";
import CLogo from "@/views/navbar/logo.vue";

const router = useRouter();

defineProps<{
  pages: string[];
}>();

const showMenu = ref(false);

router.beforeEach(() => {
  showMenu.value = false;
  return true;
});
</script>

<template lang="pug">
nav.grid(class="grid-cols-[1fr_minmax(0,_3fr)_1fr]")
  c-logo.col-start-2.justify-self-center
  c-button.justify-self-end(theme="invisible" @click="showMenu = true")
    c-icon(name="three-dots-vertical")
  .fixed.inset-0.flex.items-start.justify-end.backdrop-blur-sm.p-4.z-50(
    class="bg-black/20"
    v-show="showMenu"
    @click="showMenu = false"
  )
    .relative.rounded-md.bg-polar-lightest.max-w-full.p-5(@click="e => e.stopPropagation()")
      c-nav-links.flex-col(:pages="pages")
      .border-t.border-slate-400.my-6
      c-nav-buttons
      c-button.absolute.right-3.top-3(theme="invisible" padding="small" @click="showMenu = false")
        c-icon.text-3xl(name="x")
</template>
