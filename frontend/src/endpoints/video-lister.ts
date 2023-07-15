import { type VideoInfo } from "@/endpoints/video";

export interface VideoLister {
  listVideos(): Promise<VideoInfo[]>;
}
