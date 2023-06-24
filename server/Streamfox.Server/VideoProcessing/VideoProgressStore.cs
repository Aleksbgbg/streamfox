namespace Streamfox.Server.VideoProcessing
{
    using System;
    using System.Collections.Generic;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    public class VideoProgressStore : IVideoProgressStore, IProgressSink, IProgressRetriever
    {
        private readonly IClockFactory _clockFactory;

        private readonly Dictionary<VideoId, VideoProgressTracking> _videos =
                new Dictionary<VideoId, VideoProgressTracking>();

        public VideoProgressStore(IClockFactory clockFactory)
        {
            _clockFactory = clockFactory;
        }

        public void RegisterVideo(VideoId videoId, int totalFrames)
        {
            _videos.Add(
                    videoId,
                    new VideoProgressTracking(totalFrames, _clockFactory.CreateClock()));
        }

        public void ReportProgress(VideoId videoId, int currentFrame)
        {
            if (!_videos.ContainsKey(videoId))
            {
                return;
            }

            VideoProgressTracking videoProgressTracking = _videos[videoId];

            if (currentFrame >= videoProgressTracking.TotalFrames)
            {
                _videos.Remove(videoId);
            }
            else
            {
                if (!videoProgressTracking.Clock.IsStarted())
                {
                    videoProgressTracking.Clock.Start();
                }

                videoProgressTracking.CurrentFrame = currentFrame;
            }
        }

        public Optional<ConversionProgress> RetrieveProgress(VideoId videoId)
        {
            if (_videos.ContainsKey(videoId))
            {
                VideoProgressTracking videoProgressTracking = _videos[videoId];
                return Optional.Of(CalculateConversionProgress(videoProgressTracking));
            }

            return Optional<ConversionProgress>.Empty();
        }

        private static ConversionProgress CalculateConversionProgress(
                VideoProgressTracking videoProgressTracking)
        {
            int currentFrame = videoProgressTracking.CurrentFrame;
            int totalFrames = videoProgressTracking.TotalFrames;
            TimeSpan timeElapsed = videoProgressTracking.Clock.ElapsedTime();
            TimeSpan timeRemaining = CalculateRemainingTime(currentFrame, totalFrames, timeElapsed);

            return new ConversionProgress(currentFrame, totalFrames, timeElapsed, timeRemaining);
        }

        private static TimeSpan CalculateRemainingTime(
                int framesDone, int framesTotal, TimeSpan elapsedTime)
        {
            if (elapsedTime.Ticks == 0)
            {
                return TimeSpan.Zero;
            }

            int framesRemaining = framesTotal - framesDone;
            double framesPerTick = (double)framesDone / elapsedTime.Ticks;

            double remainingTicks = framesRemaining / framesPerTick;

            return TimeSpan.FromTicks((long)Math.Ceiling(remainingTicks));
        }

        private class VideoProgressTracking
        {
            public VideoProgressTracking(int totalFrames, IClock clock)
            {
                CurrentFrame = 0;
                TotalFrames = totalFrames;
                Clock = clock;
            }

            public int CurrentFrame { get; set; }

            public int TotalFrames { get; }

            public IClock Clock { get; }
        }
    }
}