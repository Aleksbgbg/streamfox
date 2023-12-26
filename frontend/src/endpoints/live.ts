import { type ApiResponse, apiUrl, get, patch, post, put } from "@/endpoints/request";
import type { User } from "@/endpoints/user";
import type { Id } from "@/types/id";
import { panic } from "@/utils/panic";

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

function createSession(
  id: LiveRoomId,
  offer: RTCSessionDescriptionInit,
): Promise<ApiResponse<RTCSessionDescriptionInit, RTCSessionDescriptionInit>> {
  return post<RTCSessionDescriptionInit, RTCSessionDescriptionInit>(
    `/live/rooms/${id}/session`,
    offer,
  );
}

function renegotiateSession(
  id: LiveRoomId,
  offer: RTCSessionDescriptionInit,
): Promise<ApiResponse<RTCSessionDescriptionInit, RTCSessionDescriptionInit>> {
  return put<RTCSessionDescriptionInit, RTCSessionDescriptionInit>(
    `/live/rooms/${id}/session`,
    offer,
  );
}

function trickleCandidate(
  id: LiveRoomId,
  candidate: RTCIceCandidateInit,
): Promise<ApiResponse<RTCIceCandidateInit, void>> {
  return patch<RTCIceCandidateInit, void>(`/live/rooms/${id}/session`, candidate);
}

interface Message {
  type: MessageType;
  payload: unknown;
}

enum MessageType {
  Connected,
  Disconnected,
  StartStream,
  StopStream,
}

export interface PayloadConnected {
  user: User;
}

export interface PayloadDisconnected {
  user: User;
}

export interface PayloadStartStream {
  userId: string;
  mids: string[];
}

export interface PayloadStopStream {
  userId: string;
}

export interface SessionHandlers {
  connected(payload: PayloadConnected): void;
  disconnected(payload: PayloadDisconnected): void;
  startStream(payload: PayloadStartStream): void;
  stopStream(payload: PayloadStopStream): void;
  closed(): void;
  error(messages: string[]): void;
}

async function readWebRtcMessageAsString(message: unknown): Promise<string> {
  if (message instanceof Blob) {
    return await message.text();
  } else if (message instanceof ArrayBuffer) {
    return new TextDecoder("utf-8").decode(message);
  } else {
    panic("WebRTC message event: message type was not expected");
  }
}

export function joinLiveRoom(id: LiveRoomId, handlers: SessionHandlers): RTCPeerConnection {
  const peerConnection = new RTCPeerConnection({
    iceServers: [{ urls: "stun:stun.l.google.com:19302" }],
  });

  peerConnection.addEventListener("connectionstatechange", function () {
    switch (peerConnection.connectionState) {
      case "disconnected":
      case "closed":
      case "failed":
        handlers.closed();
        break;
    }
  });
  peerConnection.addEventListener("icecandidate", function (event) {
    if (!event.candidate || event.candidate.candidate.length === 0) {
      return;
    }

    trickleCandidate(id, event.candidate.toJSON());
  });

  let negotiateSession = createSession;
  peerConnection.addEventListener("negotiationneeded", async function () {
    const offer = await peerConnection.createOffer();

    const answer = await negotiateSession(id, offer ?? panic("offer SDP is null"));
    if (!answer.success()) {
      handlers.error(answer.err().generic);
      return;
    }

    await peerConnection.setLocalDescription(offer);
    await peerConnection.setRemoteDescription(answer.value());

    negotiateSession = renegotiateSession;
  });

  const channel = peerConnection.createDataChannel("main");
  channel.addEventListener("message", async function (event) {
    const message = JSON.parse(await readWebRtcMessageAsString(event.data)) as Message;

    switch (message.type) {
      case MessageType.Connected:
        handlers.connected(message.payload as PayloadConnected);
        break;
      case MessageType.Disconnected:
        handlers.disconnected(message.payload as PayloadDisconnected);
        break;
      case MessageType.StartStream:
        handlers.startStream(message.payload as PayloadStartStream);
        break;
      case MessageType.StopStream:
        handlers.stopStream(message.payload as PayloadStopStream);
        break;
    }
  });
  channel.addEventListener("close", function () {
    handlers.closed();
  });

  return peerConnection;
}
