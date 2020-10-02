namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class BackgroundVideoProcessorTest
    {
        private readonly BackgroundVideoProcessor _backgroundVideoProcessor;

        private readonly Mock<IThumbnailExtractor> _thumbnailExtractor;

        private readonly Mock<IFormatConverter> _formatConverter;

        private readonly Mock<IMetadataExtractor> _metadataExtractor;

        private readonly Mock<IVideoFinalizer> _videoFinalizer;

        public BackgroundVideoProcessorTest()
        {
            _thumbnailExtractor = new Mock<IThumbnailExtractor>();
            _formatConverter = new Mock<IFormatConverter>();
            _metadataExtractor = new Mock<IMetadataExtractor>();
            _videoFinalizer = new Mock<IVideoFinalizer>();
            _backgroundVideoProcessor = new BackgroundVideoProcessor(
                    _thumbnailExtractor.Object,
                    _formatConverter.Object,
                    _metadataExtractor.Object,
                    _videoFinalizer.Object);
        }

        public static IEnumerable<object[]> VideoIds => new[]
        {
            new object[] { new VideoId(100) },
            new object[] { new VideoId(200) },
            new object[] { new VideoId(300) },
            new object[] { new VideoId(400) }
        };

        [Theory]
        [MemberData(nameof(VideoIds))]
        public async Task ExtractsThumbnail(VideoId videoId)
        {
            await _backgroundVideoProcessor.ProcessVideo(videoId);

            _thumbnailExtractor.Verify(extractor => extractor.ExtractThumbnail(videoId));
        }

        [Theory]
        [MemberData(nameof(VideoIds))]
        public async Task CoercesVideoToSupportedFormat(VideoId videoId)
        {
            await _backgroundVideoProcessor.ProcessVideo(videoId);

            _formatConverter.Verify(converter => converter.CoerceVideoToSupportedFormat(videoId));
        }

        [Theory]
        [MemberData(nameof(VideoIds))]
        public async Task ExtractsVideoMetadata(VideoId videoId)
        {
            await _backgroundVideoProcessor.ProcessVideo(videoId);

            _metadataExtractor.Verify(extractor => extractor.ExtractVideoMetadata(videoId));
        }

        [Theory]
        [MemberData(nameof(VideoIds))]
        public async Task FinalizesVideoProcessing(VideoId videoId)
        {
            await _backgroundVideoProcessor.ProcessVideo(videoId);

            _videoFinalizer.Verify(finalizer => finalizer.FinalizeVideoProcessing(videoId));
        }
    }
}