namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class BackgroundVideoProcessor : IBackgroundVideoProcessor
    {
        private readonly IThumbnailExtractor _thumbnailExtractor;

        private readonly IFormatConverter _formatConverter;

        private readonly IMetadataExtractor _metadataExtractor;

        private readonly IVideoFinalizer _videoFinalizer;

        public BackgroundVideoProcessor(
                IThumbnailExtractor thumbnailExtractor, IFormatConverter formatConverter,
                IMetadataExtractor metadataExtractor, IVideoFinalizer videoFinalizer)
        {
            _thumbnailExtractor = thumbnailExtractor;
            _formatConverter = formatConverter;
            _metadataExtractor = metadataExtractor;
            _videoFinalizer = videoFinalizer;
        }

        public async Task ProcessVideo(VideoId videoId)
        {
            await _thumbnailExtractor.ExtractThumbnail(videoId);
            await _formatConverter.CoerceVideoToSupportedFormat(videoId);
            await _metadataExtractor.ExtractVideoMetadata(videoId);
            await _videoFinalizer.FinalizeVideoProcessing(videoId);
        }
    }
}