import axios from "axios";
import { EndpointResolver } from "@/endpoints/endpoint-resolver";

export class Communicator {
  private readonly _endpointResolver: EndpointResolver;

  private readonly _root: string;

  public constructor(endpointResolver: EndpointResolver, root: string) {
    this._endpointResolver = endpointResolver;
    this._root = root;
  }

  public async get<T>(path = "", options = {}): Promise<T> {
    const response = await axios.get<T>(this.formatUrl(path), options);
    return response.data;
  }

  public async post<T>(path: string, data: ArrayBuffer, options = {}): Promise<T> {
    const response = await axios.post(this.formatUrl(path), data, options);
    return response.data;
  }

  private formatUrl(path: string): string {
    return this._endpointResolver.resolve(this._root + "/" + path);
  }
}
