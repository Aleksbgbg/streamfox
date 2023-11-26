<script setup lang="ts">
import { provide, reactive } from "vue";
import CToast from "@/components/toasts/toast.vue";
import {
  type RegisterToastArea,
  type Toast,
  type ToastArea,
  registerToastAreaKey,
  toastAreaKey,
} from "@/components/toasts/toaster";
import { injectStrict } from "@/utils/inject";

interface ToastWithKey extends Toast {
  key: number;
}

const toasts: ToastWithKey[] = reactive([]);

let count = 0;
const toastArea: ToastArea = {
  add(toast) {
    toasts.push({ key: ++count, ...toast });
  },
  pop() {
    toasts.splice(0, 1);
  },
};

injectStrict<RegisterToastArea>(registerToastAreaKey).register(toastArea);

provide<ToastArea>(toastAreaKey, toastArea);
</script>

<template lang="pug">
.fixed.bottom-0.flex.items-end.flex-col.gap-3.w-full.p-3.pointer-events-none
  transition-group
    c-toast(v-for="toast of toasts" :key="toast.key" :toast="toast")
</template>
