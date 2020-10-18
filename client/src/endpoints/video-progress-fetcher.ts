import { ConversionProgressResponse } from "@/endpoints/responses/conversion-progress-response";

export interface VideoProgressFetcher {
  fetchProgress(videoId: string): Promise<ConversionProgressResponse>;
}