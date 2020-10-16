namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class FormatConverterTest
    {
        private readonly FormatConverter _formatConverter;

        private readonly Mock<IIntermediateVideoPathResolver> _intermediateVideoPathResolver;

        private readonly Mock<IVideoPathResolver> _videoPathResolver;

        private readonly Mock<IVideoMetadataGrabber> _videoMetadataGrabber;

        private readonly Mock<IVideoCoercer> _videoCoercer;

        public FormatConverterTest()
        {
            _intermediateVideoPathResolver = new Mock<IIntermediateVideoPathResolver>();
            _videoPathResolver = new Mock<IVideoPathResolver>();
            _videoMetadataGrabber = new Mock<IVideoMetadataGrabber>();
            _videoCoercer = new Mock<IVideoCoercer>();
            _formatConverter = new FormatConverter(
                    _intermediateVideoPathResolver.Object,
                    _videoPathResolver.Object,
                    _videoMetadataGrabber.Object,
                    _videoCoercer.Object);
        }

        [Theory]
        [InlineData(100, "int-video-100", "video-100", VideoCodec.Other, VideoFormat.Other)]
        [InlineData(200, "int-some-video", "out-some-video", VideoCodec.Other, VideoFormat.Mp4)]
        [InlineData(300, "300-video", "out-vid-300", VideoCodec.Other, VideoFormat.Mp4)]
        [InlineData(400, "300-video", "out-vid-300", VideoCodec.H264, VideoFormat.Other)]
        [InlineData(500, "300-video", "out-vid-300", VideoCodec.Vp9, VideoFormat.Other)]
        public async Task InvalidFormat_CoercesToVp9(
                long videoIdLong, string intermediateVideoPath, string outputVideoPath,
                VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoId videoId = new VideoId(videoIdLong);
            VideoMetadata videoMetadata = new VideoMetadata(videoCodec, videoFormat);
            _intermediateVideoPathResolver
                    .Setup(resolver => resolver.ResolveIntermediateVideoPath(videoId))
                    .Returns(intermediateVideoPath);
            _videoPathResolver.Setup(resolver => resolver.ResolveVideoPath(videoId))
                              .Returns(outputVideoPath);
            _videoMetadataGrabber.Setup(grabber => grabber.GrabMetadata(intermediateVideoPath))
                                 .Returns(Task.FromResult(videoMetadata));

            await _formatConverter.CoerceVideoToSupportedFormat(videoId);

            _videoCoercer.Verify(
                    coercer => coercer.CoerceToVp9(intermediateVideoPath, outputVideoPath));
            _videoCoercer.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(100, "int-video-100", "video-100", VideoCodec.H264, VideoFormat.Mp4)]
        [InlineData(200, "int-some-video", "out-some-video", VideoCodec.Vp9, VideoFormat.Webm)]
        public async Task ValidFormat_DoesNotCoerce(
                long videoIdLong, string intermediateVideoPath, string outputVideoPath,
                VideoCodec videoCodec, VideoFormat videoFormat)
        {
            VideoId videoId = new VideoId(videoIdLong);
            VideoMetadata videoMetadata = new VideoMetadata(videoCodec, videoFormat);
            _intermediateVideoPathResolver
                    .Setup(resolver => resolver.ResolveIntermediateVideoPath(videoId))
                    .Returns(intermediateVideoPath);
            _videoPathResolver.Setup(resolver => resolver.ResolveVideoPath(videoId))
                              .Returns(outputVideoPath);
            _videoMetadataGrabber.Setup(grabber => grabber.GrabMetadata(intermediateVideoPath))
                                 .Returns(Task.FromResult(videoMetadata));

            await _formatConverter.CoerceVideoToSupportedFormat(videoId);

            _videoCoercer.Verify(
                    coercer => coercer.CopyWithoutCoercing(intermediateVideoPath, outputVideoPath));
            _videoCoercer.VerifyNoOtherCalls();
        }
    }
}