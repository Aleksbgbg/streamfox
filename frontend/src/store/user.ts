import { ref } from "vue";
import { defineStore } from "pinia";
import { getUser } from "@/endpoints/user";
import { empty } from "@/utils/optional";

export const useUserStore = defineStore("user", function () {
  const user = ref(empty());

  async function updateUser() {
    user.value = await getUser();
  }

  return {
    user,
    updateUser,
  };
});
