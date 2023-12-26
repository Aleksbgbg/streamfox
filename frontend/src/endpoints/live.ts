import { type ApiResponse, apiUrl, get, post } from "@/endpoints/request";
import type { User } from "@/endpoints/user";
import type { Id } from "@/types/id";

export function getStreamKey(): Promise<ApiResponse<void, string>> {
  return get("/live/upload/key");
}

export type LiveRoomId = Id;

export enum Visibility {
  Unlisted,
  Public,
}

export interface LiveRoomInfo {
  id: LiveRoomId;
  creator: User;
  createdAt: Date;
  name: string;
  visibility: Visibility;
  participants: number;
}

export function liveRoomThumbnail(id: LiveRoomId): string {
  return apiUrl(`/live/rooms/${id}/thumbnail`);
}

export function getLiveRooms(): Promise<ApiResponse<void, LiveRoomInfo[]>> {
  return get("/live/rooms");
}

export function getLiveRoom(id: LiveRoomId): Promise<ApiResponse<void, LiveRoomInfo>> {
  return get(`/live/rooms/${id}`);
}

export interface CreateLiveRoomInfo {
  name: string;
  visibility: Visibility;
}

export interface LiveRoomCreatedInfo {
  id: LiveRoomId;
}

export function createLiveRoom(
  info: CreateLiveRoomInfo,
): Promise<ApiResponse<CreateLiveRoomInfo, LiveRoomCreatedInfo>> {
  return post("/live/rooms", info);
}
