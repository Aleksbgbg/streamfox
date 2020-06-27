import { Communicator } from "@/endpoints/communicator";

export class CommunicatorFactory {
  public createCommunicator(endpoint: string): Communicator {
    return new Communicator("/" + endpoint);
  }
}