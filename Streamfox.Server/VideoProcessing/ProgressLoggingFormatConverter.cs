namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing.Ffmpeg;
    using Streamfox.Server.VideoManagement;

    public class ProgressLoggingFormatConverter : IFormatConverter
    {
        private readonly IFormatConverterWithLogging _formatConverterWithLogging;

        private readonly IProgressSink _progressSink;

        public ProgressLoggingFormatConverter(
                IFormatConverterWithLogging formatConverterWithLogging, IProgressSink progressSink)
        {
            _formatConverterWithLogging = formatConverterWithLogging;
            _progressSink = progressSink;
        }

        public async Task CoerceVideoToSupportedFormat(VideoId videoId)
        {
            IProgressLogger progressLogger =
                    await _formatConverterWithLogging.CoerceVideoToSupportedFormat(videoId);

            while (await progressLogger.HasMoreProgress())
            {
                ProgressReport progressReport = await progressLogger.GetNextProgress();
                await _progressSink.ReportProgress(
                        new ProgressSinkReport(videoId, progressReport.Frame));
            }
        }
    }
}