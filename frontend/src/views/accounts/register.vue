<script setup lang="ts">
import { type Ref, ref } from "vue";
import { useRouter } from "vue-router";
import CForm from "@/components/forms/form.vue";
import CFormInput from "@/components/forms/input.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { type Registration, register } from "@/endpoints/auth";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { login } from "@/utils/auth";

const router = useRouter();

const registration = {
  username: "",
  emailAddress: "",
  password: "",
  repeatPassword: "",
};

const err: Ref<ApiErr<Registration>> = ref(emptyApiErr());

async function submit() {
  err.value = emptyApiErr();

  const response = await register(registration);

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
  c-form(title="Register" @submit="submit")
    .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
      c-form-input(
        title="Username" v-model="registration.username"
        :errors="err.specific.username"
      )
      c-form-input(
        title="Email Address" v-model="registration.emailAddress"
        :errors="err.specific.emailAddress"
      )
      c-form-input(
        type="password"
        title="Password" v-model="registration.password"
        :errors="err.specific.password"
      )
      c-form-input(
        type="password"
        title="Repeat Password" v-model="registration.repeatPassword"
        :errors="err.specific.repeatPassword"
      )
    .mb-5(v-if="err.generic.length > 0")
      p.text-aurora-red(v-for="error of err.generic") {{ error }}
    button(
      class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2 mb-4"
    ) Register Account
    router-link(
      class="text-frost-deep hover:underline"
      :to="{ name: 'login' }"
    ) Already have an account? Log in now!
</template>
