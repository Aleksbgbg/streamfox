import { type VideoInfo } from "@/endpoints/responses/video-list";

export interface VideoLister {
  listVideos(): Promise<VideoInfo[]>;
}
