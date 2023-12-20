import axios, { AxiosError, type AxiosProgressEvent, type RawAxiosRequestHeaders } from "axios";
import { type Optional, empty, getValue, hasValue } from "@/types/optional";

const ISO_8601_REGEX = /^\d{4}(-\d\d(-\d\d(T\d\d:\d\d(:\d\d)?(\.\d+)?(([+-]\d\d:\d\d)|Z)?)?)?)?$/;

function isDate(value: unknown): boolean {
  if (!value) {
    return false;
  }

  return typeof value === "string" && ISO_8601_REGEX.test(value);
}

type GenericObject = { [key: string]: unknown };

function handleDates(body: GenericObject) {
  if (body === null || body === undefined || typeof body !== "object") {
    return;
  }

  for (const key of Object.keys(body)) {
    if (isDate(body[key])) {
      body[key] = new Date(body[key] as string);
    } else if (typeof body[key] === "object") {
      handleDates(body[key] as GenericObject);
    }
  }
}

const instance = axios.create();
instance.interceptors.response.use(function (response) {
  handleDates(response.data);
  return response;
});

export function apiUrl(path: string) {
  return import.meta.env.VITE_API_ENDPOINT + path;
}

export interface ApiErr<TData> {
  generic: string[];
  specific: Partial<Record<keyof TData, string[]>>;
}

export function emptyApiErr<TResponse>(): ApiErr<TResponse> {
  return {
    generic: [],
    specific: {},
  };
}

export class ApiResponse<TData, TResponse> {
  private _response: Optional<TResponse>;
  private _err: Optional<ApiErr<TData>>;

  constructor(response: Optional<TResponse>, err: Optional<ApiErr<TData>>) {
    this._response = response;
    this._err = err;
  }

  public static success<TData, TResponse>(response: TResponse): ApiResponse<TData, TResponse> {
    return new ApiResponse(response, empty());
  }

  public static failure<TData, TResponse>(err: ApiErr<TData>): ApiResponse<TData, TResponse> {
    return new ApiResponse(empty(), err);
  }

  public success(): boolean {
    return hasValue(this._response);
  }

  public value(): TResponse {
    return getValue(this._response);
  }

  public transform<T>(func: (val: TResponse) => T): T {
    return func(this.value());
  }

  public err(): ApiErr<TData> {
    return getValue(this._err);
  }
}

export async function request<TData, TResponse>(params: {
  method: "get" | "put" | "post" | "delete";
  url: string;
  data: TData;
  headers?: RawAxiosRequestHeaders;
  onUploadProgress?: (progressEvent: AxiosProgressEvent) => void;
}): Promise<ApiResponse<TData, TResponse>> {
  try {
    const response = await instance({
      method: params.method,
      url: apiUrl(params.url),
      data: params.data,
      headers: params.headers,
      onUploadProgress: params.onUploadProgress,
    });
    return ApiResponse.success(response.data);
  } catch (e) {
    const err = e as AxiosError<{ errors: ApiErr<TData> } | undefined>;

    if (err.response) {
      if (err.response.data && err.response.data.errors) {
        return ApiResponse.failure(err.response.data.errors);
      } else {
        console.error(e);
        return ApiResponse.failure({
          generic: ["Did not receive a valid response from the server."],
          specific: {},
        });
      }
    } else if (err.request) {
      console.error(e);
      return ApiResponse.failure({
        generic: ["Did not receive a response from the server."],
        specific: {},
      });
    } else {
      console.error(e);
      return ApiResponse.failure({
        generic: ["Unable to setup request."],
        specific: {},
      });
    }
  }
}

export function get<TResponse>(url: string): Promise<ApiResponse<void, TResponse>> {
  return request({ method: "get", url, data: null });
}

export function put<TData, TResponse>(
  url: string,
  data: TData,
): Promise<ApiResponse<TData, TResponse>> {
  return request({ method: "put", url, data });
}

export function post<TData, TResponse>(
  url: string,
  data: TData | null = null,
): Promise<ApiResponse<TData, TResponse>> {
  return request({ method: "post", url, data });
}

export function delete_(url: string): Promise<ApiResponse<void, void>> {
  return request({ method: "delete", url, data: null });
}
