namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoRetrievalClerkTest
    {
        private readonly VideoRetrievalClerk _videoRetrievalClerk;

        private readonly Mock<IVideoLoader> _videoLoader;

        private readonly Mock<IMetadataRetriever> _metadataRetriever;

        private readonly Mock<IVideoExistenceChecker> _videoExistenceChecker;

        public VideoRetrievalClerkTest()
        {
            _videoLoader = new Mock<IVideoLoader>();
            _metadataRetriever = new Mock<IMetadataRetriever>();
            _videoExistenceChecker = new Mock<IVideoExistenceChecker>();
            _videoRetrievalClerk = new VideoRetrievalClerk(
                    _videoLoader.Object,
                    _metadataRetriever.Object,
                    _videoExistenceChecker.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task RetrieveVideo_Existent(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            SetupStream(videoId, videoStream);

            Optional<StoredVideo> videoStreamResult =
                    await _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Same(videoStream.Value, videoStreamResult.Value.VideoStream);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task RetrieveVideo_NonExistent(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional<Stream>.Empty();
            SetupStream(videoId, videoStream);

            Optional<StoredVideo> videoStreamResult =
                    await _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.False(videoStreamResult.HasValue);
        }

        [Theory]
        [InlineData(VideoCodec.H264, VideoFormat.Mp4)]
        [InlineData(VideoCodec.Vp9, VideoFormat.Webm)]
        public async Task RetrieveVideo_RetrievesVideoMetadata(VideoCodec codec, VideoFormat format)
        {
            VideoId videoId = new VideoId(123);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            SetupStream(videoId, videoStream);
            _metadataRetriever.Setup(retriever => retriever.RetrieveMetadata(videoId))
                              .ReturnsAsync(new VideoMetadata(codec, format));

            Optional<StoredVideo> videoStreamResult =
                    await _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Equal(codec, videoStreamResult.Value.VideoMetadata.VideoCodec);
            Assert.Equal(format, videoStreamResult.Value.VideoMetadata.VideoFormat);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public void RetrieveThumbnail_UsingIdAsLabel(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            SetupStream(videoId, videoStream);

            Optional<Stream> videoStreamResult = _videoRetrievalClerk.RetrieveThumbnail(videoId);

            Assert.Same(videoStream.Value, videoStreamResult.Value);
        }

        private void SetupStream(VideoId videoId, Optional<Stream> stream)
        {
            _videoExistenceChecker.Setup(checker => checker.VideoExists(videoId))
                                  .Returns(stream.HasValue);
            if (stream.HasValue)
            {
                _videoLoader.Setup(loader => loader.LoadVideo(videoId)).Returns(stream.Value);
                _videoLoader.Setup(loader => loader.LoadThumbnail(videoId)).Returns(stream.Value);
            }
        }
    }
}