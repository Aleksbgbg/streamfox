import { endpointResolver } from "@/bootstrapper/endpoint-resolver";
import { CommunicatorFactory } from "@/endpoints/communicator-factory";
import { VideoEndpoint } from "@/endpoints/video-endpoint";
import { VideoLister } from "@/endpoints/video-lister";
import { VideoProgressFetcher } from "@/endpoints/video-progress-fetcher";
import { VideoUploader } from "@/endpoints/video-uploader";

const communicatorFactory = new CommunicatorFactory(endpointResolver);
const videoEndpoint = new VideoEndpoint(communicatorFactory.createCommunicator("videos"));

export const videoLister: VideoLister = videoEndpoint;
export const videoUploader: VideoUploader = videoEndpoint;
export const videoProgressFetcher: VideoProgressFetcher = videoEndpoint;