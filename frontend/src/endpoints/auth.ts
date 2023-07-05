import axios, { AxiosResponse } from "axios";

export interface Registration {
  username: string;
  emailAddress: string;
  password: string;
  repeatPassword: string;
}

export function register(data: Registration): Promise<AxiosResponse<{ token: string }>> {
  return axios.post("/api/auth/register", data);
}
