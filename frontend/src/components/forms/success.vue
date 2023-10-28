<script setup lang="ts">
import { computed, ref } from "vue";
import { type Optional, empty, getValue, hasValue, tryApply } from "@/types/optional";

interface Props {
  message?: string;
  timeout?: number;
}
const props = withDefaults(defineProps<Props>(), {
  message: "Success!",
  timeout: 1500,
});

const visible = ref(false);
const showMessage = ref("");

const displayMessage = computed(() => showMessage.value || props.message);

let timeoutId: Optional<ReturnType<typeof setTimeout>> = empty();
function show(message: Optional<string> = empty()) {
  if (visible.value) {
    clearTimeout(getValue(timeoutId));
  }

  visible.value = true;
  tryApply(message, (m) => (showMessage.value = m));
  timeoutId = setTimeout(reverseShow, props.timeout);
}
function reverseShow() {
  showMessage.value = "";
  visible.value = false;
}

defineExpose({ show });
</script>

<template lang="pug">
p.text-aurora-green-400.text-center(v-show="visible") {{ displayMessage }}
</template>
