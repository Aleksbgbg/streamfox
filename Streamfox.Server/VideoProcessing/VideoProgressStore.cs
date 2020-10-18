namespace Streamfox.Server.VideoProcessing
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    public class VideoProgressStore : IVideoProgressStore, IProgressSink, IProgressRetriever
    {
        private readonly ConcurrentDictionary<VideoId, ConversionProgress> _progress =
                new ConcurrentDictionary<VideoId, ConversionProgress>();

        private readonly ConcurrentDictionary<VideoId, Stopwatch> _clocks =
                new ConcurrentDictionary<VideoId, Stopwatch>();

        public Task StoreNewVideo(VideoId videoId, int frames)
        {
            ConversionProgress progress = new ConversionProgress(
                    0,
                    frames,
                    TimeSpan.Zero,
                    TimeSpan.MaxValue);
            _progress.TryAdd(videoId, progress);
            Stopwatch clock = new Stopwatch();
            _clocks.TryAdd(videoId, clock);
            return Task.CompletedTask;
        }

        public Task ReportProgress(ProgressSinkReport report)
        {
            VideoId videoId = report.VideoId;
            ConversionProgress currentProgress = _progress[videoId];

            if (report.CurrentFrame >= currentProgress.VideoDuration)
            {
                _clocks[videoId].Stop();

                _progress.TryRemove(videoId, out ConversionProgress _);
                _clocks.TryRemove(videoId, out Stopwatch _);
            }
            else
            {
                _progress[report.VideoId] = Update(report.VideoId, report.CurrentFrame);
            }

            return Task.CompletedTask;
        }

        public Optional<ConversionProgress> RetrieveConversionProgress(VideoId videoId)
        {
            if (_progress.ContainsKey(videoId))
            {
                ConversionProgress progress = _progress[videoId];
                return Optional.Of(
                        new ConversionProgress(
                                progress.CurrentFrame,
                                progress.VideoDuration,
                                _clocks[videoId].Elapsed,
                                progress.TimeRemaining));
            }

            return Optional<ConversionProgress>.Empty();
        }

        private ConversionProgress Update(VideoId videoId, int doneFrames)
        {
            ConversionProgress currentProgress = _progress[videoId];

            if (doneFrames == 0)
            {
                _clocks[videoId].Start();
            }

            double timePerFrame = (double)_clocks[videoId].ElapsedMilliseconds / doneFrames;
            int remainingFrames = currentProgress.VideoDuration - doneFrames;

            TimeSpan timeRemaining =
                    (double.IsPositiveInfinity(timePerFrame) ||
                     double.IsNaN(timePerFrame) ||
                     double.IsNegativeInfinity(timePerFrame)) ? TimeSpan.MaxValue
                            : TimeSpan.FromMilliseconds(timePerFrame * remainingFrames);
            return new ConversionProgress(
                    doneFrames,
                    currentProgress.VideoDuration,
                    TimeSpan.Zero,
                    timeRemaining);
        }
    }
}