import axios, { type AxiosProgressEvent, type AxiosResponse } from "axios";
import { apiUrl } from "@/endpoints/url";
import type { User } from "@/endpoints/user";
import type { Id } from "@/types/id";
import { panic } from "@/utils/panic";
import { type ProgressReportFunc, createProgressReporter } from "@/utils/upload-progress-reporter";

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

export function createVideo(): Promise<AxiosResponse<VideoCreatedInfo>> {
  return axios.post(apiUrl("/videos"));
}

export function videoThumbnail(id: VideoId): string {
  return apiUrl(`/videos/${id}/thumbnail`);
}

export function videoStream(id: VideoId): string {
  return apiUrl(`/videos/${id}/stream`);
}

export async function requiredWatchTimeMs(id: VideoId): Promise<number> {
  return Number.parseFloat((await axios.get(apiUrl(`/videos/${id}/required-watch-time-ms`))).data);
}

export function notifyStillWatching(id: VideoId): Promise<void> {
  return axios.post(apiUrl(`/videos/${id}/still-watching`));
}

export function uploadVideo(
  id: VideoId,
  video: ArrayBuffer,
  onReportProgress: ProgressReportFunc
): Promise<AxiosResponse<void>> {
  const reportProgress = createProgressReporter(onReportProgress);
  return axios.put(apiUrl(`/videos/${id}/stream`), video, {
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

export function updateVideo(id: VideoId, update: VideoUpdateInfo): Promise<AxiosResponse<void>> {
  return axios.put(apiUrl(`/videos/${id}/settings`), update);
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

export async function getVideoInfo(id: VideoId): Promise<VideoInfo> {
  return (await axios.get(apiUrl(`/videos/${id}/info`))).data;
}

export async function getVideos(): Promise<VideoInfo[]> {
  return (await axios.get(apiUrl("/videos"))).data;
}
