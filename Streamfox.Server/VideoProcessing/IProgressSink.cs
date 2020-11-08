namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IProgressSink
    {
        void ReportProgress(VideoId videoId, int currentFrame);
    }
}