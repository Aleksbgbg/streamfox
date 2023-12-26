<script setup lang="ts">
import { onBeforeMount, reactive } from "vue";
import CButton from "@/components/button.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormOption from "@/components/forms/select/option.vue";
import CFormSelect from "@/components/forms/select/select.vue";
import CIcon from "@/components/icon.vue";
import CFormLayout from "@/components/layout/form.vue";
import { useToaster } from "@/components/toasts/toaster";
import { getStreamKey } from "@/endpoints/live";
import { clipboardCopy } from "@/utils/clipboard";
import { getErrorMessage } from "@/utils/errors";
import { fullUrl } from "@/utils/url";

const toaster = useToaster();

const setup = reactive({
  service: 0,
  streamUrl: fullUrl("/api/live/upload/stream"),
  streamToken: "",
});

onBeforeMount(async () => {
  const response = await getStreamKey();

  if (!response.success()) {
    toaster.failureAll(response.err().generic);
    return;
  }

  setup.streamToken = response.value();
});

async function copy(name: string, value: string) {
  try {
    await clipboardCopy(value);
  } catch (e) {
    toaster.failure(getErrorMessage(e));
    return;
  }

  toaster.success(`${name} copied to clipboard.`);
}
</script>

<template lang="pug">
c-form-layout(title="Setup OBS")
  .flex.flex-col.items-center.gap-3.my-5
    p.flex.flex-col.text-center.mb-5
      span Install OBS 30.0.0 or higher.
      span Go to 'Settings', then select 'Stream'.
      span Enter the details below.
    c-form-select(
      center
      v-model="setup.service"
      title="Service"
    )
      c-form-option(title="WHIP" :value="0")
    .grid.max-w-full(class="grid-cols-[1fr_minmax(0,_auto)_1fr]")
      c-form-input.col-start-2(
        center
        title="Server"
        v-model="setup.streamUrl"
      )
      c-button(@click="copy('Server URL', setup.streamUrl)")
        c-icon(name="clipboard")
    .grid.max-w-full(class="grid-cols-[1fr_minmax(0,_auto)_1fr]")
      c-form-input.col-start-2(
        center
        type="password"
        title="Bearer Token"
        v-model="setup.streamToken"
      )
      c-button(@click="copy('Bearer token', setup.streamToken)")
        c-icon(name="clipboard")
    p.text-center.mt-5 Press 'Start Streaming' to broadcast to all live rooms that you have joined.
</template>
