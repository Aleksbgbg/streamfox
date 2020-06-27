export interface ThumbnailRetriever {
  retrieveThumbnail(videoId: string): Promise<string>;
}