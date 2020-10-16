namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class FormatConverter : IFormatConverter
    {
        private readonly IIntermediateVideoPathResolver _intermediateVideoPathResolver;

        private readonly IVideoPathResolver _videoPathResolver;

        private readonly IVideoMetadataGrabber _videoMetadataGrabber;

        private readonly IVideoCoercer _videoCoercer;

        public FormatConverter(
                IIntermediateVideoPathResolver intermediateVideoPathResolver,
                IVideoPathResolver videoPathResolver, IVideoMetadataGrabber videoMetadataGrabber,
                IVideoCoercer videoCoercer)
        {
            _intermediateVideoPathResolver = intermediateVideoPathResolver;
            _videoPathResolver = videoPathResolver;
            _videoMetadataGrabber = videoMetadataGrabber;
            _videoCoercer = videoCoercer;
        }

        public async Task CoerceVideoToSupportedFormat(VideoId videoId)
        {
            string intermediateVideoPath =
                    _intermediateVideoPathResolver.ResolveIntermediateVideoPath(videoId);
            string outputVideoPath = _videoPathResolver.ResolveVideoPath(videoId);

            VideoMetadata videoMetadata =
                    await _videoMetadataGrabber.GrabMetadata(intermediateVideoPath);

            if ((videoMetadata.VideoCodec == VideoCodec.H264 &&
                 videoMetadata.VideoFormat == VideoFormat.Mp4) ||
                (videoMetadata.VideoCodec == VideoCodec.Vp9 &&
                 videoMetadata.VideoFormat == VideoFormat.Webm))
            {
                await _videoCoercer.CopyWithoutCoercing(intermediateVideoPath, outputVideoPath);
            }
            else
            {
                await _videoCoercer.CoerceToVp9(intermediateVideoPath, outputVideoPath);
            }
        }
    }
}