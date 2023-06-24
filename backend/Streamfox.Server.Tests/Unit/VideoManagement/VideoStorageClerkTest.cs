namespace Streamfox.Server.Tests.Unit.VideoManagement
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoStorageClerkTest
    {
        private readonly Mock<IVideoIdGenerator> _videoIdGenerator;

        private readonly Mock<IVideoProcessor> _videoProcessorMock;

        private readonly VideoStorageClerk _videoStorageClerk;

        public VideoStorageClerkTest()
        {
            _videoIdGenerator = new Mock<IVideoIdGenerator>();
            _videoProcessorMock = new Mock<IVideoProcessor>();
            _videoStorageClerk = new VideoStorageClerk(
                    _videoIdGenerator.Object,
                    _videoProcessorMock.Object);

            _videoProcessorMock.Setup(
                                       processor => processor.ProcessVideo(
                                               It.IsAny<VideoId>(),
                                               It.IsAny<Stream>()))
                               .Returns(Task.FromResult(true));
        }

        public static IEnumerable<object[]> Streams => new[]
        {
            new[] { TestUtils.MockStream() },
            new[] { TestUtils.MockStream() }
        };

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task GeneratesANewLabelForVideo(long videoIdValue)
        {
            SetupVideoIdOnGeneration(videoIdValue);

            Optional<VideoId> videoId = await _videoStorageClerk.StoreVideo(TestUtils.MockStream());

            Assert.Equal(videoIdValue, videoId.Value.Value);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task ProcessesVideoId(long videoIdValue)
        {
            SetupVideoIdOnGeneration(videoIdValue);

            await _videoStorageClerk.StoreVideo(TestUtils.MockStream());

            _videoProcessorMock.Verify(
                    processor => processor.ProcessVideo(
                            new VideoId(videoIdValue),
                            It.IsAny<Stream>()));
        }

        [Theory]
        [MemberData(nameof(Streams))]
        public async Task ProcessesVideoStream(Stream stream)
        {
            SetupVideoIdOnGeneration(0);

            await _videoStorageClerk.StoreVideo(stream);

            _videoProcessorMock.Verify(
                    processor => processor.ProcessVideo(It.IsAny<VideoId>(), stream));
        }

        [Fact]
        public async Task ReturnsEmptyWhenProcessingUnsuccessful()
        {
            Stream videoStream = TestUtils.MockStream();
            _videoProcessorMock
                    .Setup(processor => processor.ProcessVideo(new VideoId(0), videoStream))
                    .Returns(Task.FromResult(false));

            Optional<VideoId> videoId = await _videoStorageClerk.StoreVideo(videoStream);

            Assert.False(videoId.HasValue);
        }

        private void SetupVideoIdOnGeneration(long videoIdValue)
        {
            _videoIdGenerator.Setup(generator => generator.GenerateVideoId())
                             .Returns(new VideoId(videoIdValue));
        }
    }
}