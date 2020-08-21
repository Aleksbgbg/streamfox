namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoRetrievalClerkTest
    {
        private readonly Mock<IVideoLoader> _videoLoader;

        private readonly Mock<IMetadataRetriever> _metadataRetriever;

        private readonly VideoRetrievalClerk _videoRetrievalClerk;

        public VideoRetrievalClerkTest()
        {
            _videoLoader = new Mock<IVideoLoader>();
            _metadataRetriever = new Mock<IMetadataRetriever>();
            _videoRetrievalClerk = new VideoRetrievalClerk(
                    _videoLoader.Object,
                    _metadataRetriever.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task RetrievesVideoUsingIdAsLabel(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            _videoLoader.Setup(loader => loader.LoadVideo(videoId.ToString())).Returns(videoStream);

            Optional<StoredVideo> videoStreamResult = await _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Same(videoStream.Value, videoStreamResult.Value.VideoStream);
        }

        [Theory]
        [InlineData(VideoCodec.H264, VideoFormat.Mp4)]
        [InlineData(VideoCodec.Vp9, VideoFormat.Webm)]
        public async Task RetrievesVideoMetadata(VideoCodec codec, VideoFormat format)
        {
            VideoId videoId = new VideoId(123);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            _videoLoader.Setup(loader => loader.LoadVideo(videoId.ToString())).Returns(videoStream);
            _metadataRetriever.Setup(retriever => retriever.RetrieveMetadata(videoId))
                              .Returns(Task.FromResult(new VideoMetadata(codec, format)));

            Optional<StoredVideo> videoStreamResult = await _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Equal(codec, videoStreamResult.Value.VideoMetadata.VideoCodec);
            Assert.Equal(format, videoStreamResult.Value.VideoMetadata.VideoFormat);
        }

        [Theory]
        [InlineData("123", "456")]
        [InlineData("456", "789")]
        public void ListsVideoIdsFromVideoLoaderLabels(string a, string b)
        {
            _videoLoader.Setup(loader => loader.ListLabels()).Returns(new[] { a, b });

            VideoId[] videos = _videoRetrievalClerk.ListVideos();

            Assert.Equal(ToVideoIds(a, b), videos);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public void RetrievesThumbnailUsingIdAsLabel(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            _videoLoader.Setup(loader => loader.LoadThumbnail(videoId.ToString()))
                        .Returns(videoStream);

            Optional<Stream> videoStreamResult = _videoRetrievalClerk.RetrieveThumbnail(videoId);

            Assert.Same(videoStream.Value, videoStreamResult.Value);
        }

        private static VideoId[] ToVideoIds(params string[] labels)
        {
            return labels.Select(long.Parse).Select(id => new VideoId(id)).ToArray();
        }
    }
}