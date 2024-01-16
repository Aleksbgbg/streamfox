import { ApiResponse, get } from "@/endpoints/request";
import type { Id } from "@/types/id";
import { type Option, none, some } from "@/types/option";

export type UserId = Id;

export interface User {
  id: string;
  username: string;
}

export async function getUser(): Promise<Option<User>> {
  const response = await get<User>("/user");

  if (response.success()) {
    return some(response.value());
  } else {
    return none();
  }
}

export function getUserById(id: UserId): Promise<ApiResponse<void, User>> {
  return get(`/users/${id}`);
}
