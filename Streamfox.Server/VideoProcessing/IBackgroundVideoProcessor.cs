namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IBackgroundVideoProcessor
    {
        void BeginProcessingVideo(VideoId videoId);
    }
}