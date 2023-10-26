<script setup lang="ts">
import { type Ref, reactive, ref } from "vue";
import { useRouter } from "vue-router";
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
      title="Username" v-model="credentials.username"
      :errors="err.specific.username"
    )
    c-form-input(
      type="password"
      title="Password" v-model="credentials.password"
      :errors="err.specific.password"
    )
    div(v-if="err.generic.length > 0")
      p.text-aurora-red.text-center(v-for="error of err.generic") {{ error }}
    button(
      class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2"
    ) Log In
    router-link(
      class="text-frost-deep text-center hover:underline"
      :to="{ name: 'register' }"
    ) Don't yet have an account? Register now!
</template>
