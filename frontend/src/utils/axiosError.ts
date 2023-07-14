import axios, { AxiosError } from "axios";
import { panicErr } from "@/utils/panic";

function asAxiosError<R>(error: unknown): error is AxiosError<R> {
  return axios.isAxiosError(error);
}

export function checkAxiosError<R>(error: unknown): AxiosError<R> {
  if (asAxiosError<R>(error)) {
    return error;
  }

  throw panicErr("axios error not found");
}
