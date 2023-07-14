<script setup lang="ts">
import { reactive } from "vue";
import { useRouter } from "vue-router";
import CForm from "@/components/forms/form.vue";
import CFormInput from "@/components/forms/input.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { requestLogin } from "@/endpoints/auth";
import { type GenericErrors, type SpecificErrors } from "@/types/errors";
import { login } from "@/utils/auth";

const router = useRouter();

const credentials = {
  username: "",
  password: "",
};

const genericErrors: GenericErrors = reactive([]);
const specificErrors: SpecificErrors = reactive({
  username: [],
  password: [],
});

function submit() {
  requestLogin(credentials)
    .then(async (response) => {
      await login();

      router.push({ name: "home" });
    })
    .catch((error) => {
      genericErrors.length = 0;
      for (const key of Object.keys(specificErrors)) {
        specificErrors[key].length = 0;
      }

      const responseErrors = error.response.data.errors;

      if (responseErrors === undefined) {
        genericErrors.push("Log in failed for an unknown reason.");
      } else if (typeof responseErrors === "object") {
        for (const key of Object.keys(specificErrors)) {
          if (responseErrors[key]) {
            specificErrors[key] = responseErrors[key];
          }
        }
      } else {
        genericErrors.push(responseErrors);
      }
    });
}
</script>

<template lang="pug">
c-center-main
  c-form(title="Login" @submit="submit")
    .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
      c-form-input(
        title="Username" v-model="credentials.username"
        :errors="specificErrors.username"
      )
      c-form-input(
        type="password"
        title="Password" v-model="credentials.password"
        :errors="specificErrors.password"
      )
    .mb-5(v-if="genericErrors.length > 0")
      p.text-aurora-red(v-for="error of genericErrors") {{ error }}
    button(
      class="bg-frost-blue hover:bg-frost-deep rounded transition duration-150 px-4 py-2 mb-4"
    ) Log In
    router-link(
      class="text-frost-deep hover:underline"
      :to="{ name: 'register' }"
    ) Don't yet have an account? Register now!
</template>
