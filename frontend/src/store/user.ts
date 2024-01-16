import { type Ref, shallowRef } from "vue";
import { defineStore } from "pinia";
import { type User, getUser } from "@/endpoints/user";
import { type Option, none } from "@/types/option";

export const useUserStore = defineStore("user", function () {
  const user: Ref<Option<User>> = shallowRef(none());

  async function updateUser() {
    user.value = await getUser();
  }

  return {
    user,
    updateUser,
  };
});
