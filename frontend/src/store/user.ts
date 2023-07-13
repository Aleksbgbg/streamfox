import { type Ref, ref } from "vue";
import { defineStore } from "pinia";
import { type User, getUser } from "@/endpoints/user";
import { type Optional, empty } from "@/utils/optional";

export const useUserStore = defineStore("user", function () {
  const user: Ref<Optional<User>> = ref(empty());

  async function updateUser() {
    user.value = await getUser();
  }

  return {
    user,
    updateUser,
  };
});
