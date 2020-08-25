import { communicatorFactory } from "@/bootstrapper/communicator-factory";
import { Communicator } from "@/endpoints/communicator";
import { VideoList } from "@/endpoints/responses/video-list";
import { VideoMetadata } from "@/endpoints/responses/video-metadata";
import { VideoLister } from "@/endpoints/video-lister";
import { VideoUploader } from "@/endpoints/video-uploader";
import { UploadedDataReport } from "@/helpers/uploaded-data-report";

export class VideoEndpoint implements VideoLister, VideoUploader {
  private readonly _communicator: Communicator;

  public constructor() {
    this._communicator = communicatorFactory.createCommunicator("videos");
  }

  public async listVideos(): Promise<VideoList> {
    return await this._communicator.get<VideoList>();
  }

  public async uploadVideo(video: ArrayBuffer, onUploadProgress: (uploadedDataReport: UploadedDataReport) => void): Promise<VideoMetadata> {
    return await this._communicator.post<VideoMetadata>("", video, {
      headers: { "Content-Type": "application/octet-stream" },
      onUploadProgress(progressEvent: any) {
        onUploadProgress({
          loaded: progressEvent.loaded,
          total: progressEvent.total
        });
      }
    });
  }
}