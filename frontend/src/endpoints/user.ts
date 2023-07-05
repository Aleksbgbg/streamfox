import axios from "axios";
import { Optional, empty } from "@/utils/optional";

export interface User {
  id: string;
  username: string;
}

export async function getUser(): Promise<Optional<User>> {
  try {
    const response = await axios.get("/api/user");
    return response.data;
  } catch {
    return empty();
  }
}
