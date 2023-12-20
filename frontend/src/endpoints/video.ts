import type { AxiosProgressEvent } from "axios";
import { type ApiResponse, apiUrl, get, post, put, request } from "@/endpoints/request";
import type { User } from "@/endpoints/user";
import type { Id } from "@/types/id";
import type { UploadReportFunc } from "@/utils/upload-progress";

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

export interface WatchConditions {
  percentage: number;
  remainingBytes: number;
  remainingTimeMs: number;
}

export function getWatchConditions(id: VideoId): Promise<ApiResponse<void, WatchConditions>> {
  return get(`/videos/${id}/watch-conditions`);
}

export function postView(id: VideoId): Promise<ApiResponse<void, void>> {
  return post(`/videos/${id}/views`);
}

export interface ContentRange {
  start: number;
  end: number;
  size: number;
}

export function uploadVideo(
  id: VideoId,
  video: ArrayBuffer,
  range: ContentRange,
  reportProgress: UploadReportFunc,
): Promise<ApiResponse<void, void>> {
  return request({
    method: "put",
    url: `/videos/${id}/stream`,
    data: video,
    headers: { "Content-Range": `bytes ${range.start}-${range.end}/${range.size}` },
    onUploadProgress(progressEvent: AxiosProgressEvent) {
      reportProgress({
        uploadedBytes: range.start + progressEvent.loaded,
        totalBytes: range.size,
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
  update: VideoUpdateInfo,
): Promise<ApiResponse<VideoUpdateInfo, VideoUpdateInfo>> {
  return put(`/videos/${id}/settings`, update);
}

export interface VideoInfo {
  id: VideoId;
  creator: User;
  durationSecs: number;
  uploadedAt: Date;
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
