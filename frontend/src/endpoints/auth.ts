import axios, { type AxiosResponse } from "axios";
import { apiUrl } from "@/endpoints/url";

export type Authorization = AxiosResponse<{ token: string }>;

export interface Registration {
  username: string;
  emailAddress: string;
  password: string;
  repeatPassword: string;
}

export function register(data: Registration): Promise<Authorization> {
  return axios.post(apiUrl("/auth/register"), data);
}

export interface Credentials {
  username: string;
  password: string;
}

export function requestLogin(data: Credentials): Promise<Authorization> {
  return axios.post(apiUrl("/auth/login"), data);
}
