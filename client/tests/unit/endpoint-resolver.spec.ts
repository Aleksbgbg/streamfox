import { EndpointResolver } from "@/endpoints/endpoint-resolver";

describe("EndpointResolver", () => {
  it("resolves regular url", () => {
    const resolver = new EndpointResolver("/streamfox/api");

    const resolved = resolver.resolve("/videos/123/thumbnail");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves extra trailing slashes url", () => {
    const resolver = new EndpointResolver("/streamfox/api/");

    const resolved = resolver.resolve("/videos/123/thumbnail/");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves no leading slashes url", () => {
    const resolver = new EndpointResolver("streamfox/api");

    const resolved = resolver.resolve("videos/123/thumbnail");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves too many slashes url", () => {
    const resolver = new EndpointResolver("///streamfox///api///");

    const resolved = resolver.resolve("///videos//////123///thumbnail///");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves empty url", () => {
    const resolver = new EndpointResolver("///streamfox///api///");

    const resolved = resolver.resolve("");

    expect(resolved).toBe("/streamfox/api");
  });
});