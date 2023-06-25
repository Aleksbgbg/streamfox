import { EndpointResolver } from "@/endpoints/endpoint-resolver";

export const endpointResolver = new EndpointResolver(import.meta.env.VITE_API_ENDPOINT);
