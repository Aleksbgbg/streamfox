import { useUserStore } from "@/store/user";

function deleteCookie(name: string) {
  document.cookie = name + "=; Path=/; Expires=Thu, 01 Jan 1970 00:00:01 GMT;";
}

export async function refreshLoginStatus() {
  await useUserStore().updateUser();
}

export async function login() {
  await refreshLoginStatus();
}

export function logout() {
  deleteCookie("Authorization");
  refreshLoginStatus();
}
