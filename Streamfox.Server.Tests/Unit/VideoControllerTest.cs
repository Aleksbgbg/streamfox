namespace Streamfox.Server.Tests.Unit
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Moq;

    using Xunit;

    public class VideoControllerTest
    {
        private readonly Mock<IVideoClerk> _videoClerkMock;

        private readonly VideoController _videoController;

        public VideoControllerTest()
        {
            _videoClerkMock = new Mock<IVideoClerk>();
            _videoController = new VideoController(_videoClerkMock.Object);
        }

        [Fact]
        public async Task PostVideoTwice_ReturnSeparateCreatedResultsWithVideoGetUri()
        {
            Stream videoStream1 = new MemoryStream(new byte[] { 1, 2, 3 });
            Stream videoStream2 = new MemoryStream(new byte[] { 4, 5, 6 });
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream1))
                           .Returns(Task.FromResult(new VideoId(123)));
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream2))
                           .Returns(Task.FromResult(new VideoId(456)));

            CreatedResult result1 = await _videoController.PostVideo(videoStream1);
            CreatedResult result2 = await _videoController.PostVideo(videoStream2);

            Assert.Equal("/videos/123", result1.Location);
            Assert.Equal("/videos/456", result2.Location);
        }

        [Fact]
        public void GetExistingVideo_ReturnsVideoStream()
        {
            VideoId videoId = new VideoId(123);
            Stream videoStream = TestUtil.MockStream();
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional<Stream>.Of(videoStream));

            StreamResult result = _videoController.GetVideo(videoId) as StreamResult;

            Assert.IsType<StreamResult>(result);
            Assert.Equal(videoStream, result.Stream);
        }

        [Fact]
        public void GetMissingVideo_ReturnsNotFound()
        {
            VideoId videoId = new VideoId(123);
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional<Stream>.Empty());

            NotFoundResult result = _videoController.GetVideo(videoId) as NotFoundResult;

            Assert.IsType<NotFoundResult>(result);
        }
    }
}