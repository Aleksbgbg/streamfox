import { VideoMetadata } from "@/endpoints/responses/video-metadata";
import { UploadedDataReport } from "@/helpers/uploaded-data-report";

export interface VideoUploader {
  uploadVideo(
    video: ArrayBuffer,
    onUploadProgress: (uploadedDataReport: UploadedDataReport) => void
  ): Promise<VideoMetadata>;
}
