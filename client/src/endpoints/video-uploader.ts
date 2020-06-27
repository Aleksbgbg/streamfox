import { VideoMetadata } from "@/endpoints/responses/video-metadata";

export interface VideoUploader {
  uploadVideo(video: ArrayBuffer): Promise<VideoMetadata>;
}