namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing;

    using Xunit;

    public class FfmpegTest
    {
        private readonly Mock<IProcessRunner> _processRunner;

        private readonly Ffmpeg _ffmpeg;

        public FfmpegTest()
        {
            _processRunner = new Mock<IProcessRunner>();
            _ffmpeg = new Ffmpeg(_processRunner.Object);
        }

        [Fact]
        public async Task ExtractThumbnail_CallsProcessCorrectly()
        {
            await _ffmpeg.ExtractThumbnail("video", "thumbnail");

            _processRunner.Verify(
                    runner => runner.RunFfmpeg(
                            "-i \"video\" -vframes 1 -q:v 5 -vf scale=-1:225 -f singlejpeg \"thumbnail\""));
        }

        [Fact]
        public async Task ConvertToVp9_CallsProcessCorrectly()
        {
            await _ffmpeg.ConvertToVp9Webm("video", "output");

            _processRunner.Verify(
                    runner => runner.RunFfmpeg("-i \"video\" -c:v vp9 -f webm \"output\""));
        }

        [Fact]
        public async Task ConvertToMp4_CallsProcessCorrectly()
        {
            await _ffmpeg.ConvertToMp4("video", "output");

            _processRunner.Verify(
                    runner => runner.RunFfmpeg("-i \"video\" -c:v copy -f mp4 \"output\""));
        }

        [Theory]
        [ClassData(typeof(VideoMetadataTestData))]
        public async Task GrabVideoCodec_DetectsCodecsAccurately(
                string ffprobeResult, VideoMetadata expectedMetadata)
        {
            _processRunner.Setup(runner => runner.RunFfprobe("-v quiet -show_streams -show_format -print_format json \"video\""))
                          .Returns(Task.FromResult(ffprobeResult));

            VideoMetadata metadata = await _ffmpeg.GrabVideoMetadata("video");

            Assert.Equal(expectedMetadata, metadata);
        }
    }
}