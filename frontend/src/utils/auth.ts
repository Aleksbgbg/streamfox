import axios from "axios";
import { localGet, localSet } from "@/utils/local-storage";

const TOKEN_KEY = "token";

export function refreshLoginStatus() {
  axios.defaults.headers.common["Authorization"] = localGet(TOKEN_KEY, "");
}

export function login(token: string) {
  localSet(TOKEN_KEY, token);
  refreshLoginStatus();
}
