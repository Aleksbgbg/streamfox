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
            _videoLoaderMock.Setup(loader => loader.LoadVideo(It.IsAny<string>()))
                            .Returns(Optional<Stream>.Empty());

            _videoRetrievalClerk = new VideoRetrievalClerk(_videoLoaderMock.Object);
        }

        [Fact]
        public void RetrievesExistingVideo()
        {
            VideoId videoId = new VideoId(123);
            Stream stream = TestUtil.MockStream();
            _videoLoaderMock.Setup(loader => loader.LoadVideo("123"))
                            .Returns(Optional<Stream>.Of(stream));

            Optional<Stream> videoStream = _videoRetrievalClerk.RetrieveVideo(videoId);

            Assert.True(videoStream.HasValue, "No video retrieved");
            Assert.Equal(stream, videoStream.Value);
        }
    }
}