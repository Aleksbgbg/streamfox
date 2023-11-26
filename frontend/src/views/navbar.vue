<script setup lang="ts">
import { useRoute } from "vue-router";
import logo from "@/assets/logo.svg?component";
import CButton from "@/components/button.vue";
import CUserBadge from "@/components/user/badge.vue";
import { useUserStore } from "@/store/user";
import { getValue, hasValue } from "@/types/optional";
import { logout } from "@/utils/auth";
import { returnUrl } from "@/utils/navigation";

const route = useRoute();

const store = useUserStore();

const pageNames = ["home", "upload"];
</script>

<template lang="pug">
nav.flex.items-center.p-5
  router-link.flex.items-center.mr-5(:to="{ name: 'home' }")
    logo.fill-white.w-14.-mt-4
    h1.font-bold.text-2xl.-ml-4 Streamfox
  ul.grow.flex.gap-3
    li.capitalize(v-for="name of pageNames")
      router-link.text-xl.text-neutral-400.py-5.px-2(
        class="[&.router-link-exact-active]:text-white [&.router-link-exact-active]:font-bold"
        :to="{ name }"
      ) {{ name }}
  .flex.items-center.gap-3
    template(v-if="hasValue(store.user)")
      c-user-badge(:user="getValue(store.user)")
      c-button(theme="invisible" @click="logout")
        span Log Out
        i.bi-box-arrow-in-right
    template(v-else)
      c-button(
        element="router-link"
        theme="invisible"
        :to="{ name: 'login', query: { returnUrl: returnUrl(route) } }"
      )
        i.bi-box-arrow-in-right
        span Log In
      c-button(
        element="router-link"
        theme="invisible"
        :to="{ name: 'register', query: { returnUrl: returnUrl(route) } }"
      )
        i.bi-person
        span Register
</template>
