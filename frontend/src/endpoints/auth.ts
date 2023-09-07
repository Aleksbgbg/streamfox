import { ApiResponse, post } from "@/endpoints/request";

export interface Authorization {
  token: string;
}

export interface Registration {
  username: string;
  emailAddress: string;
  password: string;
  repeatPassword: string;
}

export function register(data: Registration): Promise<ApiResponse<Registration, Authorization>> {
  return post("/auth/register", data);
}

export interface Credentials {
  username: string;
  password: string;
}

export function requestLogin(data: Credentials): Promise<ApiResponse<Credentials, Authorization>> {
  return post("/auth/login", data);
}
