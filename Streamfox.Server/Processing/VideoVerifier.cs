namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class VideoVerifier : IVideoVerifier
    {
        private readonly IIntermediateVideoPathResolver _intermediateVideoPathResolver;

        private readonly IVideoMetadataGrabber _videoMetadataGrabber;

        public VideoVerifier(
                IIntermediateVideoPathResolver intermediateVideoPathResolver,
                IVideoMetadataGrabber videoMetadataGrabber)
        {
            _intermediateVideoPathResolver = intermediateVideoPathResolver;
            _videoMetadataGrabber = videoMetadataGrabber;
        }

        public async Task<bool> IsValidVideo(VideoId videoId)
        {
            string videoPath = _intermediateVideoPathResolver.ResolveIntermediateVideoPath(videoId);

            VideoMetadata videoMetadata = await _videoMetadataGrabber.GrabMetadata(videoPath);

            return videoMetadata.VideoCodec != VideoCodec.Invalid;
        }
    }
}