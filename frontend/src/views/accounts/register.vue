<script setup lang="ts">
import { type Ref, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import CErrors from "@/components/forms/errors.vue";
import CFormInput from "@/components/forms/input.vue";
import CFormLayout from "@/components/layout/form.vue";
import { type Registration, register } from "@/endpoints/auth";
import { type ApiErr, emptyApiErr } from "@/endpoints/request";
import { login } from "@/utils/auth";

const router = useRouter();

const registration = reactive({
  username: "",
  emailAddress: "",
  password: "",
  repeatPassword: "",
});

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
c-form-layout(title="Register")
  form.flex.flex-col.items-center.gap-3.my-5(@submit.prevent="submit")
    c-form-input(
      center
      title="Username"
      v-model="registration.username"
      :errors="err.specific.username"
    )
    c-form-input(
      center
      title="Email Address"
      v-model="registration.emailAddress"
      :errors="err.specific.emailAddress"
    )
    c-form-input(
      center
      type="password"
      title="Password"
      v-model="registration.password"
      :errors="err.specific.password"
    )
    c-form-input(
      center
      type="password"
      title="Repeat Password"
      v-model="registration.repeatPassword"
      :errors="err.specific.repeatPassword"
    )
    c-errors(center :errors="err.generic")
    button(
      class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2"
    ) Register Account
    router-link(
      class="text-frost-deep text-center hover:underline"
      :to="{ name: 'login' }"
    ) Already have an account? Log in now!
</template>
