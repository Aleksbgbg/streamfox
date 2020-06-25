namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.IO;
    using System.Linq;

    using Moq;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoRetrievalClerkTest
    {
        private readonly Mock<IVideoLoader> _videoLoaderMock;

        private readonly VideoRetrievalClerk _videoRetrievalClerk;

        public VideoRetrievalClerkTest()
        {
            _videoLoaderMock = new Mock<IVideoLoader>();
            _videoRetrievalClerk = new VideoRetrievalClerk(_videoLoaderMock.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public void RetrievesVideoUsingIdAsLabel(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional.Of(TestUtils.MockStream());
            _videoLoaderMock.Setup(loader => loader.LoadVideo(videoId.ToString()))
                            .Returns(videoStream);

            Optional<Stream> videoStreamResult = _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Same(videoStream, videoStreamResult);
        }

        [Theory]
        [InlineData("123", "456")]
        [InlineData("456", "789")]
        public void ListsVideoIdsFromVideoLoaderLabels(string a, string b)
        {
            _videoLoaderMock.Setup(loader => loader.ListLabels())
                            .Returns(new[] { a, b });

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
            _videoLoaderMock.Setup(loader => loader.LoadThumbnail(videoId.ToString()))
                            .Returns(videoStream);

            Optional<Stream> videoStreamResult = _videoRetrievalClerk.RetrieveThumbnail(videoId);

            Assert.Same(videoStream, videoStreamResult);
        }

        private static VideoId[] ToVideoIds(params string[] labels)
        {
            return labels.Select(long.Parse).Select(id => new VideoId(id)).ToArray();
        }
    }
}