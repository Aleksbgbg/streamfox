namespace Streamfox.Server.Tests.Unit.Processing.Ffmpeg
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing;
    using Streamfox.Server.Processing.Ffmpeg;

    using Xunit;

    public class FfmpegProcessVideoOperationRunnerTest
    {
        private readonly Mock<IFfmpegProcessRunner> _processRunner;

        private readonly FfmpegProcessVideoOperationRunner _ffmpegProcessVideoOperationRunner;

        public FfmpegProcessVideoOperationRunnerTest()
        {
            _processRunner = new Mock<IFfmpegProcessRunner>();
            _ffmpegProcessVideoOperationRunner = new FfmpegProcessVideoOperationRunner(_processRunner.Object);
        }

        [Fact]
        public async Task ExtractThumbnail_CallsProcessCorrectly()
        {
            await _ffmpegProcessVideoOperationRunner.ExtractThumbnail("video", "thumbnail");

            _processRunner.Verify(
                    runner => runner.RunFfmpeg(
                            "-i \"video\" -vframes 1 -q:v 2 -vf scale=-1:225 -f singlejpeg \"thumbnail\""));
        }

        [Fact]
        public async Task ConvertToVp9_CallsProcessCorrectly()
        {
            await _ffmpegProcessVideoOperationRunner.ConvertToVp9Webm("video", "output");

            _processRunner.Verify(
                    runner => runner.RunFfmpeg("-i \"video\" -c:v vp9 -crf 30 -b:v 0 -f webm \"output\""));
        }

        [Theory]
        [ClassData(typeof(VideoMetadataTestData))]
        public async Task GrabVideoCodec_DetectsCodecsAccurately(
                string ffprobeResult, VideoMetadata expectedMetadata)
        {
            _processRunner.Setup(runner => runner.RunFfprobe("-v quiet -show_streams -show_format -print_format json \"video\""))
                          .Returns(Task.FromResult(ffprobeResult));

            VideoMetadata metadata = await _ffmpegProcessVideoOperationRunner.GrabVideoMetadata("video");

            Assert.Equal(expectedMetadata, metadata);
        }
    }
}