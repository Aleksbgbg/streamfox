<script setup lang="ts">
import { type Ref, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import CButton from "@/components/button.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormLayout from "@/components/layout/form.vue";
import { useToaster } from "@/components/toasts/toaster";
import { type Credentials, requestLogin } from "@/endpoints/auth";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { login } from "@/utils/auth";
import { navigateBackOrHome } from "@/utils/navigation";

const route = useRoute();
const router = useRouter();

const toaster = useToaster();

const credentials = reactive({
  username: "",
  password: "",
});

const err: Ref<ApiErr<Credentials>> = ref(emptyApiErr());

async function submit() {
  err.value = emptyApiErr();

  const response = await requestLogin(credentials);

  if (!response.success()) {
    err.value = response.err();
    toaster.failureAll(err.value.generic);
    return;
  }

  await login();
  navigateBackOrHome(router, route);
}
</script>

<template lang="pug">
c-form-layout(title="Login")
  form.flex.flex-col.items-center.gap-3.my-5(@submit.prevent="submit")
    c-form-input(
      center
      title="Username"
      v-model="credentials.username"
      :errors="err.specific.username"
    )
    c-form-input(
      center
      title="Password"
      type="password"
      v-model="credentials.password"
      :errors="err.specific.password"
    )
    c-button Log In
    router-link(
      class="text-frost-deep text-center hover:underline"
      :to="{ name: 'register' }"
    ) Don't yet have an account? Register now!
</template>
