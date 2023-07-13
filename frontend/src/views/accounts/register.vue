<script setup lang="ts">
import { reactive } from "vue";
import { useRouter } from "vue-router";
import CFormInput from "@/components/forms/input.vue";
import CCenterMain from "@/components/layout/center-main.vue";
import { register } from "@/endpoints/auth";
import { type GenericErrors, type SpecificErrors } from "@/types/errors";
import { login } from "@/utils/auth";

const router = useRouter();

const registration = {
  username: "",
  emailAddress: "",
  password: "",
  repeatPassword: "",
};

const genericErrors: GenericErrors = reactive([]);
const specificErrors: SpecificErrors = reactive({
  username: [],
  emailAddress: [],
  password: [],
  repeatPassword: [],
});

function submit() {
  register(registration)
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
  .bg-polar-darkest.border.border-frost-deep.rounded-t.overflow-hidden.m-5(class="w-2/3")
    h2.bg-polar-dark.border-b.border-frost-deep.text-center.py-2 Register
    form.flex.flex-col.items-center.my-4(@submit.prevent="submit")
      .grid.gap-4.mb-4(class="grid-cols-[auto_1fr] w-3/4")
        c-form-input(
          title="Username" v-model="registration.username"
          :errors="specificErrors.username"
        )
        c-form-input(
          title="Email Address" v-model="registration.emailAddress"
          :errors="specificErrors.emailAddress"
        )
        c-form-input(
          type="password"
          title="Password" v-model="registration.password"
          :errors="specificErrors.password"
        )
        c-form-input(
          type="password"
          title="Repeat Password" v-model="registration.repeatPassword"
          :errors="specificErrors.repeatPassword"
        )
      .mb-5(v-if="genericErrors.length > 0")
        p.text-aurora-red(v-for="error of genericErrors") {{ error }}
      button(
        class="bg-frost-blue hover:bg-frost-deep \
        rounded \
        transition duration-150 \
        px-4 py-2 mb-4"
      ) Register Account
      router-link(
        class="text-frost-deep hover:underline"
        :to="{ name: 'login' }"
      ) Already have an account? Log in now!
</template>
