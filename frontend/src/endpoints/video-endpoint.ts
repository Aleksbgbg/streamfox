import { Communicator } from "@/endpoints/communicator";
import { type ConversionProgressResponse } from "@/endpoints/responses/conversion-progress-response";
import { type VideoInfo } from "@/endpoints/responses/video-list";
import { type VideoMetadata } from "@/endpoints/responses/video-metadata";
import { type VideoLister } from "@/endpoints/video-lister";
import { type VideoProgressFetcher } from "@/endpoints/video-progress-fetcher";
import { type VideoUploader } from "@/endpoints/video-uploader";
import { type UploadedDataReport } from "@/utils/uploaded-data-report";

export class VideoEndpoint implements VideoLister, VideoUploader, VideoProgressFetcher {
  private readonly _communicator: Communicator;

  public constructor(communicator: Communicator) {
    this._communicator = communicator;
  }

  public async listVideos(): Promise<VideoInfo[]> {
    return await this._communicator.get<VideoInfo[]>();
  }

  public async uploadVideo(
    video: ArrayBuffer,
    onUploadProgress: (uploadedDataReport: UploadedDataReport) => void
  ): Promise<VideoMetadata> {
    return await this._communicator.post<VideoMetadata>("", video, {
      headers: { "Content-Type": "application/octet-stream" },
      onUploadProgress(progressEvent: UploadedDataReport) {
        onUploadProgress({
          loaded: progressEvent.loaded,
          total: progressEvent.total,
        });
      },
    });
  }

  public async fetchProgress(videoId: string): Promise<ConversionProgressResponse> {
    return await this._communicator.get<ConversionProgressResponse>(`${videoId}/progress`);
  }
}
