namespace Streamfox.Server.VideoManagement
{
    using System;

    public class ConversionProgress
    {
        public ConversionProgress(
                int currentFrame, int videoDuration, TimeSpan timeElapsed, TimeSpan timeRemaining)
        {
            CurrentFrame = currentFrame;
            VideoDuration = videoDuration;
            TimeElapsed = timeElapsed;
            TimeRemaining = timeRemaining;
        }

        public int CurrentFrame { get; }

        public int VideoDuration { get; }

        public TimeSpan TimeElapsed { get; }

        public TimeSpan TimeRemaining { get; }
    }
}