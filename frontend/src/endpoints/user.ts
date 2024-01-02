import { ApiResponse, get } from "@/endpoints/request";
import type { Id } from "@/types/id";
import { type Optional, empty } from "@/types/optional";

export type UserId = Id;

export interface User {
  id: string;
  username: string;
}

export async function getUser(): Promise<Optional<User>> {
  const response = await get<User>("/user");

  if (response.success()) {
    return response.value();
  } else {
    return empty();
  }
}

export function getUserById(id: UserId): Promise<ApiResponse<void, User>> {
  return get(`/users/${id}`);
}
