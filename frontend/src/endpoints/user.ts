import axios from "axios";
import { apiUrl } from "@/endpoints/url";
import type { Id } from "@/types/id";
import { type Optional, empty } from "@/types/optional";

export type UserId = Id;

export interface User {
  id: string;
  username: string;
}

export async function getUser(): Promise<Optional<User>> {
  try {
    const response = await axios.get(apiUrl("/user"));
    return response.data;
  } catch {
    return empty();
  }
}
