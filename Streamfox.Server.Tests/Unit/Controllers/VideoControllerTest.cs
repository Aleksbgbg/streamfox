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
        public async Task PostVideoCreatesVideoUrlsBasedOnVideoId(
                VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream))
                           .Returns(Task.FromResult(videoId));

            CreatedResult result = await _videoController.PostVideo(videoStream);

            Assert.Equal($"/videos/{videoId}", result.Location);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public async Task PostVideoReturnsVideoMetadataInResponse(
                VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.StoreVideo(videoStream))
                           .Returns(Task.FromResult(videoId));

            CreatedResult result = await _videoController.PostVideo(videoStream);

            VideoMetadata videoMetadata = result.Value as VideoMetadata;
            Assert.IsType<VideoMetadata>(videoMetadata);
            Assert.Equal(videoId.ToString(), videoMetadata.VideoId);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetExistingVideoReturnsVideoStream(VideoId videoId, Stream videoStream)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional<Stream>.Of(videoStream));

            StreamResult result = _videoController.GetVideo(videoId) as StreamResult;

            Assert.IsType<StreamResult>(result);
            Assert.Equal(videoStream, result.Stream);
        }

        [Theory]
        [MemberData(nameof(VideoCases))]
        public void GetMissingVideoReturnsNotFound(VideoId videoId, Stream _)
        {
            _videoClerkMock.Setup(clerk => clerk.RetrieveVideo(videoId))
                           .Returns(Optional<Stream>.Empty());

            IActionResult result = _videoController.GetVideo(videoId);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}