namespace Streamfox.Server.Controllers.Responses
{
    using System;

    public class ConversionProgressResponse
    {
        public ConversionProgressResponse()
        {
        }

        public ConversionProgressResponse(
                bool isCompleted, TimeSpan currentFrame, TimeSpan videoDuration,
                double doneFraction, TimeSpan timeElapsed, TimeSpan timeRemaining)
        {
            IsCompleted = isCompleted;
            CurrentFrame = currentFrame;
            VideoDuration = videoDuration;
            DoneFraction = doneFraction;
            TimeElapsed = timeElapsed;
            TimeRemaining = timeRemaining;
        }

        public bool IsCompleted { get; }

        public TimeSpan CurrentFrame { get; }

        public TimeSpan VideoDuration { get; }

        public double DoneFraction { get; }

        public TimeSpan TimeElapsed { get; }

        public TimeSpan TimeRemaining { get; }
    }
}