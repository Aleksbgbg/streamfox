<script setup lang="ts">
import { type Ref, ref } from "vue";
import { useRouter } from "vue-router";
import CForm from "@/components/forms/form.vue";
import CFormInput from "@/components/forms/input.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { type Credentials, requestLogin } from "@/endpoints/auth";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { login } from "@/utils/auth";

const router = useRouter();

const credentials = {
  username: "",
  password: "",
};

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
c-center-main
  c-form(title="Login" @submit="submit")
    .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
      c-form-input(
        title="Username" v-model="credentials.username"
        :errors="err.specific.username"
      )
      c-form-input(
        type="password"
        title="Password" v-model="credentials.password"
        :errors="err.specific.password"
      )
    .mb-5(v-if="err.generic.length > 0")
      p.text-aurora-red(v-for="error of err.generic") {{ error }}
    button(
      class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2 mb-4"
    ) Log In
    router-link(
      class="text-frost-deep hover:underline"
      :to="{ name: 'register' }"
    ) Don't yet have an account? Register now!
</template>
