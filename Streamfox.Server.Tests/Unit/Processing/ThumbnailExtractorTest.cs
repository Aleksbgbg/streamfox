namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class ThumbnailExtractorTest
    {
        private readonly ThumbnailExtractor _thumbnailExtractor;

        private readonly Mock<IIntermediateVideoPathResolver> _intermediatePathResolver;

        private readonly Mock<IThumbnailPathResolver> _thumbnailPathResolver;

        private readonly Mock<IFileSystemThumbnailExtractor> _fileSystemThumbnailExtractor;

        public ThumbnailExtractorTest()
        {
            _intermediatePathResolver = new Mock<IIntermediateVideoPathResolver>();
            _thumbnailPathResolver = new Mock<IThumbnailPathResolver>();
            _fileSystemThumbnailExtractor = new Mock<IFileSystemThumbnailExtractor>();
            _thumbnailExtractor = new ThumbnailExtractor(
                    _intermediatePathResolver.Object,
                    _thumbnailPathResolver.Object,
                    _fileSystemThumbnailExtractor.Object);
        }

        [Theory]
        [InlineData(100, "video-100", "thumbnail-100")]
        [InlineData(200, "video-path", "thumbnail-path")]
        [InlineData(300, "video", "thumbnail")]
        public async Task ExtractsThumbnailFromFilePaths(
                long videoIdLong, string videoPath, string thumbnailPath)
        {
            VideoId videoId = new VideoId(videoIdLong);
            _intermediatePathResolver
                    .Setup(resolver => resolver.ResolveIntermediateVideoPath(videoId))
                    .Returns(videoPath);
            _thumbnailPathResolver.Setup(resolver => resolver.ResolveThumbnailPath(videoId))
                                  .Returns(thumbnailPath);

            await _thumbnailExtractor.ExtractThumbnail(videoId);

            _fileSystemThumbnailExtractor.Verify(
                    extractor => extractor.ExtractThumbnail(videoPath, thumbnailPath));
        }
    }
}