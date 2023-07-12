import { Communicator } from "@/endpoints/communicator";
import { ConversionProgressResponse } from "@/endpoints/responses/conversion-progress-response";
import { VideoInfo } from "@/endpoints/responses/video-list";
import { VideoMetadata } from "@/endpoints/responses/video-metadata";
import { VideoLister } from "@/endpoints/video-lister";
import { VideoProgressFetcher } from "@/endpoints/video-progress-fetcher";
import { VideoUploader } from "@/endpoints/video-uploader";
import { UploadedDataReport } from "@/utils/uploaded-data-report";

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
