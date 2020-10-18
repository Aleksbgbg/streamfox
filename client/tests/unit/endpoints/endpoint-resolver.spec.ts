import { EndpointResolver } from "@/endpoints/endpoint-resolver";

describe("EndpointResolver", () => {
  it("resolves regular thumbnailUrl", () => {
    const resolver = new EndpointResolver("/streamfox/api");

    const resolved = resolver.resolve("/videos/123/thumbnail");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves extra trailing slashes thumbnailUrl", () => {
    const resolver = new EndpointResolver("/streamfox/api/");

    const resolved = resolver.resolve("/videos/123/thumbnail/");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves no leading slashes thumbnailUrl", () => {
    const resolver = new EndpointResolver("streamfox/api");

    const resolved = resolver.resolve("videos/123/thumbnail");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves too many slashes thumbnailUrl", () => {
    const resolver = new EndpointResolver("///streamfox///api///");

    const resolved = resolver.resolve("///videos//////123///thumbnail///");

    expect(resolved).toBe("/streamfox/api/videos/123/thumbnail");
  });

  it("resolves empty thumbnailUrl", () => {
    const resolver = new EndpointResolver("///streamfox///api///");

    const resolved = resolver.resolve("");

    expect(resolved).toBe("/streamfox/api");
  });
});