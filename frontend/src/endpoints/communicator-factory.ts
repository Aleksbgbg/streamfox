import { Communicator } from "@/endpoints/communicator";
import { EndpointResolver } from "@/endpoints/endpoint-resolver";

export class CommunicatorFactory {
  private readonly _endpointResolver: EndpointResolver;

  public constructor(endpointResolver: EndpointResolver) {
    this._endpointResolver = endpointResolver;
  }

  public createCommunicator(endpoint: string): Communicator {
    return new Communicator(this._endpointResolver, endpoint);
  }
}