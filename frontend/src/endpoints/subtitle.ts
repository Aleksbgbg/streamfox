import { type ApiResponse, apiUrl, delete_, get, post, put } from "@/endpoints/request";
import type { VideoId } from "@/endpoints/video";
import type { Id } from "@/types/id";

export type SubtitleId = Id;

export interface SubtitleInfo {
  subtitlesExtracted: boolean;
}

export function getSubtitleInfo(videoId: VideoId): Promise<ApiResponse<void, SubtitleInfo>> {
  return get(`/videos/${videoId}/subtitles/info`);
}

export interface Subtitle {
  id: Id;
  name: string;
}

export function extractSubtitles(videoId: VideoId): Promise<ApiResponse<void, Subtitle[]>> {
  return post(`/videos/${videoId}/subtitles/extract`);
}

export function getAllSubtitles(videoId: VideoId): Promise<ApiResponse<void, Subtitle[]>> {
  return get(`/videos/${videoId}/subtitles`);
}

export function subtitleContentUrl(videoId: VideoId, subtitleId: SubtitleId): string {
  return apiUrl(`/videos/${videoId}/subtitles/${subtitleId}/content`);
}

export function getSubtitleContent(
  videoId: VideoId,
  subtitleId: SubtitleId
): Promise<ApiResponse<void, string>> {
  return get(`/videos/${videoId}/subtitles/${subtitleId}/content`);
}

export function createSubtitle(
  videoId: VideoId,
  fileContent = ""
): Promise<ApiResponse<void, Subtitle>> {
  return post(`/videos/${videoId}/subtitles`, fileContent);
}

export interface UpdateSubtitleInfo {
  name: string;
  content: string;
}

export function updateSubtitle(
  videoId: VideoId,
  subtitleId: SubtitleId,
  update: UpdateSubtitleInfo
): Promise<ApiResponse<UpdateSubtitleInfo, void>> {
  return put(`/videos/${videoId}/subtitles/${subtitleId}`, update);
}

export function deleteSubtitle(
  videoId: VideoId,
  subtitleId: SubtitleId
): Promise<ApiResponse<void, void>> {
  return delete_(`/videos/${videoId}/subtitles/${subtitleId}`);
}
