namespace Streamfox.Server.VideoProcessing
{
    using System;
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
            try
            {
                await _formatConverter.CoerceVideoToSupportedFormat(videoId);
                await _metadataExtractor.ExtractVideoMetadata(videoId);
                await _videoFinalizer.FinalizeVideoProcessing(videoId);
            }
            catch (Exception e)
            {
                Console.WriteLine("== Error in video processing");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}