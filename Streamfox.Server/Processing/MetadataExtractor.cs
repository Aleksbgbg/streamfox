namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class MetadataExtractor : IMetadataExtractor
    {
        private readonly IVideoPathResolver _videoPathResolver;

        private readonly IVideoMetadataGrabber _videoMetadataGrabber;

        private readonly IMetadataSaver _metadataSaver;

        public MetadataExtractor(
                IVideoPathResolver videoPathResolver, IVideoMetadataGrabber videoMetadataGrabber,
                IMetadataSaver metadataSaver)
        {
            _videoPathResolver = videoPathResolver;
            _videoMetadataGrabber = videoMetadataGrabber;
            _metadataSaver = metadataSaver;
        }

        public async Task ExtractVideoMetadata(VideoId videoId)
        {
            string videoPath = _videoPathResolver.ResolveVideoPath(videoId);

            VideoMetadata videoMetadata = await _videoMetadataGrabber.GrabMetadata(videoPath);

            await _metadataSaver.SaveMetadata(videoId, videoMetadata);
        }
    }
}