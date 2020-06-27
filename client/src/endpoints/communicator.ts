import axios from "axios";

export class Communicator {
  private readonly _root: string;

  public constructor(root: string) {
    this._root = root;
  }

  public async get<T>(path = "", options = { }): Promise<T> {
    const response = await axios.get<T>(this.formatUrl(path), options);
    return response.data;
  }

  public async post<T>(path: string, data: ArrayBuffer, options = { }): Promise<T> {
    const response = await axios.post(this.formatUrl(path), data, options);
    return response.data;
  }

  private formatUrl(path: string): string {
    return this._root + path;
  }
}