import { markRaw, shallowRef } from "vue";
import { defineStore } from "pinia";
import { type User, getUser } from "@/endpoints/user";
import { none } from "@/types/option";

export const useUserStore = defineStore("user", function () {
  const user = shallowRef(markRaw(none<User>()));

  async function updateUser() {
    user.value = await getUser();
  }

  return {
    user,
    updateUser,
  };
});
