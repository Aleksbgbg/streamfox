namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.IO;

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
        public void RetrievesVideoByUsingIdAsLabel(long videoIdValue)
        {
            VideoId videoId = new VideoId(videoIdValue);
            Optional<Stream> videoStream = Optional<Stream>.Of(TestUtils.MockStream());
            _videoLoaderMock.Setup(loader => loader.LoadVideo(videoId.ToString()))
                            .Returns(videoStream);

            Optional<Stream> videoStreamResult = _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.Same(videoStream, videoStreamResult);
        }
    }
}