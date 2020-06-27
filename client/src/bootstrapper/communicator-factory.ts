import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import { CommunicatorFactory } from "@/endpoints/communicator-factory";

export const communicatorFactory: CommunicatorFactory = new CommunicatorFactory(endpointResolver);