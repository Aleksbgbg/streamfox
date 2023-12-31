<script setup lang="ts">
import { useRoute } from "vue-router";
import CButton from "@/components/button.vue";
import CDropdownButton from "@/components/dropdown/button.vue";
import CDropdownItem from "@/components/dropdown/item.vue";
import CIcon from "@/components/icon.vue";
import CUserBadge from "@/components/user/badge.vue";
import { useUserStore } from "@/store/user";
import { getValue, hasValue } from "@/types/optional";
import { logout } from "@/utils/auth";
import { returnUrl } from "@/utils/navigation";

const route = useRoute();

const store = useUserStore();
</script>

<template lang="pug">
.flex.items-center.gap-3
  template(v-if="hasValue(store.user)")
    c-dropdown-button(theme="purple")
      c-user-badge(:user="getValue(store.user)")
      template(#dropdown)
        c-dropdown-item(@click="logout") Log Out
  template(v-else)
    c-button.min-w-fit(
      theme="invisible"
      :to="{ name: 'login', query: { returnUrl: returnUrl(route) } }"
    )
      c-icon(name="box-arrow-in-right")
      span Log In
    c-button.min-w-fit(
      theme="invisible"
      :to="{ name: 'register', query: { returnUrl: returnUrl(route) } }"
    )
      c-icon(name="person")
      span Register
</template>
