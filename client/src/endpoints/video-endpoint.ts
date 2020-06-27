import { communicatorFactory } from "@/bootstrapper/communicator-factory";
import { Communicator } from "@/endpoints/communicator";
import { VideoList } from "@/endpoints/responses/video-list";
import { VideoMetadata } from "@/endpoints/responses/video-metadata";
import { ThumbnailRetriever } from "@/endpoints/thumbnail-retriever";
import { VideoLister } from "@/endpoints/video-lister";
import { VideoUploader } from "@/endpoints/video-uploader";

export class VideoEndpoint implements VideoLister, ThumbnailRetriever, VideoUploader {
  private readonly _communicator: Communicator;

  public constructor() {
    this._communicator = communicatorFactory.createCommunicator("videos");
  }

  public async listVideos(): Promise<VideoList> {
    return await this._communicator.get<VideoList>();
  }

  public async retrieveThumbnail(videoId: string): Promise<string> {
    const response = await this._communicator.get<string>(`/${videoId}/thumbnail`, { responseType: "arraybuffer" });
    const base64String = Buffer.from(response, "binary").toString("base64");

    return "data:image/jpg;base64," + base64String;
  }

  public async uploadVideo(video: ArrayBuffer): Promise<VideoMetadata> {
    return await this._communicator.post<VideoMetadata>("", video, { headers: { "Content-Type": "application/octet-stream" } });
  }
}