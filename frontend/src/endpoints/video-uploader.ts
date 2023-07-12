import { type VideoMetadata } from "@/endpoints/responses/video-metadata";
import { type UploadedDataReport } from "@/utils/uploaded-data-report";

export interface VideoUploader {
  uploadVideo(
    video: ArrayBuffer,
    onUploadProgress: (uploadedDataReport: UploadedDataReport) => void
  ): Promise<VideoMetadata>;
}
