namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public class ProgressSinkReport
    {
        public ProgressSinkReport(VideoId videoId, int currentFrame)
        {
            VideoId = videoId;
            CurrentFrame = currentFrame;
        }

        public VideoId VideoId { get; }

        public int CurrentFrame { get; }
    }
}