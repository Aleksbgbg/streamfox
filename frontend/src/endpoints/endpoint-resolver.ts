function joinFragments(fragments: string[]): string {
  return (
    "/" +
    fragments
      .flatMap((fragment) => fragment.split("/"))
      .filter((fragment) => fragment.length > 0)
      .join("/")
  );
}

export class EndpointResolver {
  private readonly _root: string;

  public constructor(root: string) {
    this._root = root;
  }

  public resolve(target = ""): string {
    return joinFragments([this._root, target]);
  }
}
