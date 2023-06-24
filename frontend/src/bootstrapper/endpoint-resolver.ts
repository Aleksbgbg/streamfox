import { EndpointResolver } from "@/endpoints/endpoint-resolver";

export const endpointResolver = new EndpointResolver(process.env.VUE_APP_API_ENDPOINT);