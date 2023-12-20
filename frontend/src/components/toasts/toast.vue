<script setup lang="ts">
import { onMounted } from "vue";
import CIcon from "@/components/icon.vue";
import { type Toast, type ToastArea, ToastType, toastAreaKey } from "@/components/toasts/toaster";
import { injectStrict } from "@/utils/inject";
import { sleep } from "@/utils/sleep";

const props = defineProps<{
  toast: Toast;
}>();

const styles = {
  [ToastType.Success]: {
    border: "border-green-500",
    bg: "bg-green-300",
    text: "text-green-700",
    mark: "check",
  },
  [ToastType.Failure]: {
    border: "border-red-500",
    bg: "bg-red-300",
    text: "text-red-700",
    mark: "x",
  },
}[props.toast.type];

const toastArea = injectStrict<ToastArea>(toastAreaKey);
onMounted(async () => {
  await sleep(5000);
  toastArea.pop();
});
</script>

<template lang="pug">
.flex.items-center.rounded.border.px-4.py-1.transition-transform.translate-x-0(
  :class="[styles.border, styles.bg, 'md:max-w-[50%] 2xl:max-w-[33%] [&.v-enter-from]:translate-x-[200%] [&.v-leave-to]:translate-x-[200%]']"
)
  c-icon.text-green-700.text-2xl.-ml-2.mr-1(:class="styles.text" :name="styles.mark")
  p.text-black.line-clamp-2 {{ toast.message }}
</template>
