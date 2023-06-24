namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoVerifierTest
    {
        private readonly VideoVerifier _videoVerifier;

        private readonly Mock<IIntermediateVideoPathResolver> _intermediateVideoPathResolver;

        private readonly Mock<IVideoMetadataGrabber> _videoMetadataGrabber;

        public VideoVerifierTest()
        {
            _intermediateVideoPathResolver = new Mock<IIntermediateVideoPathResolver>();
            _videoMetadataGrabber = new Mock<IVideoMetadataGrabber>();
            _videoVerifier = new VideoVerifier(
                    _intermediateVideoPathResolver.Object,
                    _videoMetadataGrabber.Object);
        }

        [Theory]
        [InlineData(100, "video-100", VideoCodec.H264, VideoFormat.Mp4)]
        [InlineData(200, "200", VideoCodec.Vp9, VideoFormat.Webm)]
        [InlineData(300, "some-random-video", VideoCodec.Other, VideoFormat.Other)]
        public async Task ValidVideo_ReturnsTrue(
                long videoIdLong, string videoPath, VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoId videoId = new VideoId(videoIdLong);
            VideoMetadata videoMetadata = new VideoMetadata(videoCodec, videoFormat);
            _intermediateVideoPathResolver
                    .Setup(resolver => resolver.ResolveIntermediateVideoPath(videoId))
                    .Returns(videoPath);
            _videoMetadataGrabber.Setup(grabber => grabber.GrabMetadata(videoPath))
                                 .Returns(Task.FromResult(videoMetadata));

            bool isValid = await _videoVerifier.IsValidVideo(videoId);

            Assert.True(isValid);
        }

        [Theory]
        [InlineData(100, "video-100", VideoCodec.Invalid, VideoFormat.Mp4)]
        [InlineData(200, "200", VideoCodec.Invalid, VideoFormat.Webm)]
        [InlineData(300, "some-random-video-300", VideoCodec.Invalid, VideoFormat.Other)]
        public async Task InvalidVideo_ReturnsFalse(
                long videoIdLong, string videoPath, VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoId videoId = new VideoId(videoIdLong);
            VideoMetadata videoMetadata = new VideoMetadata(videoCodec, videoFormat);
            _intermediateVideoPathResolver
                    .Setup(resolver => resolver.ResolveIntermediateVideoPath(videoId))
                    .Returns(videoPath);
            _videoMetadataGrabber.Setup(grabber => grabber.GrabMetadata(videoPath))
                                 .Returns(Task.FromResult(videoMetadata));

            bool isValid = await _videoVerifier.IsValidVideo(videoId);

            Assert.False(isValid);
        }
    }
}