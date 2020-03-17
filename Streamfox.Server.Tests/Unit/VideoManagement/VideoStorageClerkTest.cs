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
            _videoStorageClerk = new VideoStorageClerk(_videoIdGenerator.Object, _videoSaverMock.Object);
        }

        [Fact]
        public async Task GeneratesANewLabelForEachVideo()
        {
            _videoIdGenerator.SetupSequence(generator => generator.GenerateVideoId())
                             .Returns(new VideoId(534))
                             .Returns(new VideoId(780));

            VideoId videoId1 = await _videoStorageClerk.StoreVideo(TestUtils.MockStream());
            VideoId videoId2 = await _videoStorageClerk.StoreVideo(TestUtils.MockStream());

            Assert.Equal(534, videoId1.Value);
            Assert.Equal(780, videoId2.Value);
        }

        [Fact]
        public async Task SavesEachVideoWithTheCorrectLabel()
        {
            Stream videoStream1 = TestUtils.MockStream();
            Stream videoStream2 = TestUtils.MockStream();
            _videoIdGenerator.SetupSequence(generator => generator.GenerateVideoId())
                             .Returns(new VideoId(123))
                             .Returns(new VideoId(456));

            await _videoStorageClerk.StoreVideo(videoStream1);
            await _videoStorageClerk.StoreVideo(videoStream2);

            _videoSaverMock.Verify(saver => saver.SaveVideo("123", videoStream1));
            _videoSaverMock.Verify(saver => saver.SaveVideo("456", videoStream2));
        }
    }
}