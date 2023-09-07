import type { AxiosProgressEvent } from "axios";
import { type ApiResponse, apiUrl, get, post, put, request } from "@/endpoints/request";
import type { User } from "@/endpoints/user";
import type { Id } from "@/types/id";
import { panic } from "@/utils/panic";
import { type ProgressReportFunc, createProgressReporter } from "@/utils/upload-progress";

export type VideoId = Id;

export enum Visibility {
  Private,
  Unlisted,
  Public,
}

export interface VideoCreatedInfo {
  id: VideoId;
  name: string;
  description: string;
  visibility: Visibility;
}

export function createVideo(): Promise<ApiResponse<void, VideoCreatedInfo>> {
  return post("/videos", null);
}

export function videoThumbnail(id: VideoId): string {
  return apiUrl(`/videos/${id}/thumbnail`);
}

export function videoStream(id: VideoId): string {
  return apiUrl(`/videos/${id}/stream`);
}

export function requiredWatchTimeMs(id: VideoId): Promise<ApiResponse<void, string>> {
  return get(`/videos/${id}/required-watch-time-ms`);
}

export function notifyStillWatching(id: VideoId): Promise<ApiResponse<void, void>> {
  return post(`/videos/${id}/still-watching`);
}

export function uploadVideo(
  id: VideoId,
  video: ArrayBuffer,
  onReportProgress: ProgressReportFunc
): Promise<ApiResponse<void, void>> {
  const reportProgress = createProgressReporter(onReportProgress);
  return request({
    method: "put",
    url: `/videos/${id}/stream`,
    data: video,
    onUploadProgress(progressEvent: AxiosProgressEvent) {
      reportProgress({
        uploadedBytes: progressEvent.loaded,
        totalBytes: progressEvent.total ?? panic("total video upload bytes unavailable"),
      });
    },
  });
}

export interface VideoUpdateInfo {
  name: string;
  description: string;
  visibility: Visibility;
}

export function updateVideo(
  id: VideoId,
  update: VideoUpdateInfo
): Promise<ApiResponse<VideoUpdateInfo, VideoUpdateInfo>> {
  return put(`/videos/${id}/settings`, update);
}

export interface VideoInfo {
  id: VideoId;
  creator: User;
  durationSecs: number;
  name: string;
  description: string;
  visibility: Visibility;
  views: number;
  likes: number;
  dislikes: number;
}

export function getVideoInfo(id: VideoId): Promise<ApiResponse<void, VideoInfo>> {
  return get(`/videos/${id}/info`);
}

export function getVideos(): Promise<ApiResponse<void, VideoInfo[]>> {
  return get("/videos");
}
