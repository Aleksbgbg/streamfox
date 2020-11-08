namespace Streamfox.Server.Tests.Unit.VideoProcessing
{
    using System.Threading.Tasks;

    using Moq;

    using Streamfox.Server.Processing.Ffmpeg;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    using Xunit;

    public class ProgressLoggingFormatConverterTest
    {
        private readonly ProgressLoggingFormatConverter _progressLoggingFormatConverter;

        private readonly Mock<IFormatConverterWithLogging> _formatConverterWithLogging;

        private readonly Mock<IProgressSink> _progressSink;

        public ProgressLoggingFormatConverterTest()
        {
            _formatConverterWithLogging = new Mock<IFormatConverterWithLogging>();
            _progressSink = new Mock<IProgressSink>();
            _progressLoggingFormatConverter = new ProgressLoggingFormatConverter(
                    _formatConverterWithLogging.Object,
                    _progressSink.Object);
        }

        [Theory]
        [InlineData(123)]
        [InlineData(456)]
        public async Task LogsProgress(long videoIdLong)
        {
            VideoId videoId = new VideoId(videoIdLong);
            IProgressLogger progressLogger = SetupProgressLogging(
                    new ProgressReport(0),
                    new ProgressReport(125),
                    new ProgressReport(250),
                    new ProgressReport(300),
                    new ProgressReport(400),
                    new ProgressReport(450),
                    new ProgressReport(499),
                    new ProgressReport(500));
            _formatConverterWithLogging
                    .Setup(converter => converter.CoerceVideoToSupportedFormat(videoId))
                    .ReturnsAsync(progressLogger);

            await _progressLoggingFormatConverter.CoerceVideoToSupportedFormat(videoId);

            VerifyProgress(videoId, 0);
            VerifyProgress(videoId, 125);
            VerifyProgress(videoId, 250);
            VerifyProgress(videoId, 300);
            VerifyProgress(videoId, 400);
            VerifyProgress(videoId, 450);
            VerifyProgress(videoId, 499);
            VerifyProgress(videoId, 500);
        }

        private static IProgressLogger SetupProgressLogging(params ProgressReport[] progressReports)
        {
            Mock<IProgressLogger> progressLogger = new Mock<IProgressLogger>();

            SetupHasMoreProgressSequence(progressLogger, progressReports);
            SetupGetNextProgressSequence(progressLogger, progressReports);

            return progressLogger.Object;
        }

        private static void SetupHasMoreProgressSequence(
                Mock<IProgressLogger> progressLogger, ProgressReport[] progressReports)
        {
            var hasMoreProgressSequence =
                    progressLogger.SetupSequence(logger => logger.HasMoreProgress());

            for (int index = 0; index < progressReports.Length; index++)
            {
                hasMoreProgressSequence.ReturnsAsync(true);
            }

            hasMoreProgressSequence.ReturnsAsync(false);
        }

        private static void SetupGetNextProgressSequence(
                Mock<IProgressLogger> progressLogger, ProgressReport[] progressReports)
        {
            var getNextProgressSequence =
                    progressLogger.SetupSequence(logger => logger.GetNextProgress());

            foreach (ProgressReport progressReport in progressReports)
            {
                getNextProgressSequence.ReturnsAsync(progressReport);
            }
        }

        private void VerifyProgress(VideoId videoId, int currentFrame)
        {
            _progressSink.Verify(sink => sink.ReportProgress(videoId, currentFrame));
        }
    }
}