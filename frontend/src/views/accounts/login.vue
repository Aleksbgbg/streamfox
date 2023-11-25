<script setup lang="ts">
import { type Ref, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import CButton from "@/components/button.vue";
import CErrors from "@/components/forms/errors.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormLayout from "@/components/layout/form.vue";
import { type Credentials, requestLogin } from "@/endpoints/auth";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { login } from "@/utils/auth";

const router = useRouter();

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
    return;
  }

  await login();
  router.push({ name: "home" });
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
    c-errors(center :errors="err.generic")
    c-button Log In
    router-link(
      class="text-frost-deep text-center hover:underline"
      :to="{ name: 'register' }"
    ) Don't yet have an account? Register now!
</template>
