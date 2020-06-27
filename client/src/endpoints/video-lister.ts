import { VideoList } from "@/endpoints/responses/video-list";

export interface VideoLister {
  listVideos(): Promise<VideoList>;
}