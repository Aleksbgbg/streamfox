namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class BackgroundVideoProcessor : IBackgroundVideoProcessor
    {
        private readonly IFormatConverter _formatConverter;

        private readonly IMetadataExtractor _metadataExtractor;

        private readonly IVideoFinalizer _videoFinalizer;

        public BackgroundVideoProcessor(
                IFormatConverter formatConverter, IMetadataExtractor metadataExtractor,
                IVideoFinalizer videoFinalizer)
        {
            _formatConverter = formatConverter;
            _metadataExtractor = metadataExtractor;
            _videoFinalizer = videoFinalizer;
        }

        public async Task ProcessVideo(VideoId videoId)
        {
            await _formatConverter.CoerceVideoToSupportedFormat(videoId);
            await _metadataExtractor.ExtractVideoMetadata(videoId);
            await _videoFinalizer.FinalizeVideoProcessing(videoId);
        }
    }
}