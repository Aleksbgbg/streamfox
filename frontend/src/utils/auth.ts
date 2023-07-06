import axios from "axios";
import { useUserStore } from "@/store/user";
import { localGet, localSet } from "@/utils/local-storage";

const TOKEN_KEY = "token";

export function refreshLoginStatus() {
  axios.defaults.headers.common["Authorization"] = localGet(TOKEN_KEY, "");
  useUserStore().updateUser();
}

export function login(token: string) {
  localSet(TOKEN_KEY, token);
  refreshLoginStatus();
}
