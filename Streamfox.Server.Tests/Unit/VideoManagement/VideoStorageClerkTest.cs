namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoStorageClerkTest
    {
        private readonly Mock<IVideoIdGenerator> _videoIdGenerator;

        private readonly Mock<IVideoSaver> _videoSaverMock;

        private readonly VideoStorageClerk _videoStorageClerk;

        public VideoStorageClerkTest()
        {
            _videoIdGenerator = new Mock<IVideoIdGenerator>();
            _videoSaverMock = new Mock<IVideoSaver>();
            _videoStorageClerk = new VideoStorageClerk(
                    _videoIdGenerator.Object,
                    _videoSaverMock.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task GeneratesANewLabelForVideo(long videoIdValue)
        {
            SetupVideoIdOnGeneration(videoIdValue);

            VideoId videoId = await _videoStorageClerk.StoreVideo(TestUtils.MockStream());

            Assert.Equal(videoIdValue, videoId.Value);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task SavesVideoUsingIdAsLabel(long videoIdValue)
        {
            Stream videoStream = TestUtils.MockStream();
            SetupVideoIdOnGeneration(videoIdValue);

            await _videoStorageClerk.StoreVideo(videoStream);

            _videoSaverMock.Verify(saver => saver.SaveVideo(videoIdValue.ToString(), videoStream));
        }

        private void SetupVideoIdOnGeneration(long videoIdValue)
        {
            _videoIdGenerator.Setup(generator => generator.GenerateVideoId())
                             .Returns(new VideoId(videoIdValue));
        }
    }
}