namespace Streamfox.Server.Tests.Unit.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Moq;

    using Streamfox.Server.Controllers;
    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.Controllers.Results;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

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

        public static IEnumerable<object[]> VideoCases => new[]
        {
            new object[] { new VideoId(100), new MemoryStream(new byte[] { 1, 2, 3 }) },
            new object[] { new VideoId(200), new MemoryStream(new byte[] { 4, 5, 6 }) }
        };

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task PostVideo_CreatesVideoUrlsFromVideoId(
                VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream))
                           .Returns(Task.FromResult(Optional.Of(videoId)));

            CreatedResult result = await _videoController.PostVideo(videoStream) as CreatedResult;

            Assert.Equal($"/videos/{videoId}", result.Location);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task PostVideo_ReturnsVideoMetadataInResponse(
                VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream))
                           .Returns(Task.FromResult(Optional.Of(videoId)));

            CreatedResult result = await _videoController.PostVideo(videoStream) as CreatedResult;

            VideoMetadata videoMetadata = result.Value as VideoMetadata;
            Assert.IsType<VideoMetadata>(videoMetadata);
            Assert.Equal(videoId.ToString(), videoMetadata.VideoId);
        }

        [Fact]
        public async Task PostVideo_EmptyId_ReturnsBadRequest()
        {
            Stream videoStream = TestUtils.MockStream();
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream))
                           .Returns(Task.FromResult(Optional<VideoId>.Empty()));

            IActionResult result = await _videoController.PostVideo(videoStream);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetVideo_ExistingVideo_ReturnsVideoStream(VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional.Of(videoStream));

            StreamResult result = _videoController.GetVideo(videoId) as StreamResult;

            Assert.IsType<StreamResult>(result);
            Assert.Equal(videoStream, result.Stream);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetVideo_MissingVideo_ReturnsNotFound(VideoId videoId, Stream _)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional<Stream>.Empty());

            IActionResult result = _videoController.GetVideo(videoId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void GetVideos_ReturnsListOfIds()
        {
            _videoClerkMock.Setup(clerk => clerk.ListVideos())
                           .Returns(new[] { new VideoId(100), new VideoId(200) });

            OkObjectResult result = _videoController.GetVideos();

            VideoList resultValue = result.Value as VideoList;
            Assert.NotNull(resultValue);
            Assert.Equal("100", resultValue.VideoIds[0]);
            Assert.Equal("200", resultValue.VideoIds[1]);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetThumbnail_ExistingThumbnail_ReturnsImageStream(VideoId videoId, Stream imageStream)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveThumbnail(videoId))
                           .Returns(Optional.Of(imageStream));

            StreamResult result = _videoController.GetThumbnail(videoId) as StreamResult;

            Assert.IsType<StreamResult>(result);
            Assert.Equal(imageStream, result.Stream);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetThumbnail_MissingThumbnail_ReturnsNotFound(VideoId videoId, Stream _)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveThumbnail(videoId))
                           .Returns(Optional<Stream>.Empty());

            IActionResult result = _videoController.GetThumbnail(videoId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}