import axios, { type AxiosProgressEvent, type AxiosResponse } from "axios";
import { panic } from "@/utils/panic";
import type { UploadedDataReport } from "@/utils/uploaded-data-report";

export type VideoId = string;

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
  return axios.post("/api/videos");
}

export function videoThumbnail(id: VideoId): string {
  return `/api/videos/${id}/thumbnail`;
}

export function uploadVideo(
  id: VideoId,
  video: ArrayBuffer,
  onUploadProgress: (uploadedDataReport: UploadedDataReport) => void
): Promise<AxiosResponse<void>> {
  return axios.put(`/api/videos/${id}/stream`, video, {
    onUploadProgress(progressEvent: AxiosProgressEvent) {
      onUploadProgress({
        loaded: progressEvent.loaded,
        total: progressEvent.total ?? panic("total video upload bytes unavailable"),
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
  return axios.put(`/api/videos/${id}/settings`, update);
}
