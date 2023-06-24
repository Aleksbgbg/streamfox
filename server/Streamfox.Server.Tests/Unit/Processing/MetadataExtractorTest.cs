namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class MetadataExtractorTest
    {
        private readonly MetadataExtractor _metadataExtractor;

        private readonly Mock<IVideoPathResolver> _videoPathResolver;

        private readonly Mock<IVideoMetadataGrabber> _videoMetadataGrabber;

        private readonly Mock<IMetadataSaver> _metadataSaver;

        public MetadataExtractorTest()
        {
            _videoPathResolver = new Mock<IVideoPathResolver>();
            _videoMetadataGrabber = new Mock<IVideoMetadataGrabber>();
            _metadataSaver = new Mock<IMetadataSaver>();
            _metadataExtractor = new MetadataExtractor(
                    _videoPathResolver.Object,
                    _videoMetadataGrabber.Object,
                    _metadataSaver.Object);
        }

        [Theory]
        [InlineData(100, "video-path-100", VideoCodec.H264, VideoFormat.Mp4)]
        [InlineData(200, "video", VideoCodec.Vp9, VideoFormat.Webm)]
        [InlineData(300, "random-video-path", VideoCodec.Other, VideoFormat.Other)]
        public async Task SavesGrabbedMetadata(
                long videoIdLong, string videoPath, VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoId videoId = new VideoId(videoIdLong);
            VideoMetadata videoMetadata = new VideoMetadata(videoCodec, videoFormat);
            _videoPathResolver.Setup(resolver => resolver.ResolveVideoPath(videoId))
                              .Returns(videoPath);
            _videoMetadataGrabber.Setup(grabber => grabber.GrabMetadata(videoPath))
                                 .Returns(Task.FromResult(videoMetadata));

            await _metadataExtractor.ExtractVideoMetadata(videoId);

            _metadataSaver.Verify(saver => saver.SaveMetadata(videoId, videoMetadata));
        }
    }
}