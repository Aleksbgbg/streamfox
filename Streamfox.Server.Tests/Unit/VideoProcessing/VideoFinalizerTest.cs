namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class VideoFinalizerTest
    {
        private readonly VideoFinalizer _videoFinalizer;

        private readonly Mock<IIntermediateVideoDeleter> _intermediateVideoDeleter;

        public VideoFinalizerTest()
        {
            _intermediateVideoDeleter = new Mock<IIntermediateVideoDeleter>();
            _videoFinalizer = new VideoFinalizer(_intermediateVideoDeleter.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task DeletesIntermediateVideo(long videoIdLong)
        {
            VideoId videoId = new VideoId(videoIdLong);

            await _videoFinalizer.FinalizeVideoProcessing(videoId);

            _intermediateVideoDeleter.Verify(deleter => deleter.DeleteIntermediateVideo(videoId));
        }
    }
}