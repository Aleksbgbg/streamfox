namespace Streamfox.Server.Controllers.Responses
{
    using Streamfox.Server.VideoManagement;

    public class ConversionProgressResponse
    {
        public ConversionProgressResponse()
        {
        }

        public ConversionProgressResponse(ConversionProgress conversionProgress)
        {
            IsCompleted = conversionProgress.CurrentFrame == conversionProgress.VideoDuration;
            CurrentFrame = conversionProgress.CurrentFrame;
            VideoDuration = conversionProgress.VideoDuration;
            DoneFraction = (double)conversionProgress.CurrentFrame / conversionProgress.VideoDuration;
            TimeElapsed = conversionProgress.TimeElapsed.TotalSeconds;
            TimeRemaining = conversionProgress.TimeRemaining.TotalSeconds;
        }

        public bool IsCompleted { get; set; }

        public int CurrentFrame { get; set; }

        public int VideoDuration { get; set; }

        public double DoneFraction { get; set; }

        public double TimeElapsed { get; set; }

        public double TimeRemaining { get; set; }
    }
}