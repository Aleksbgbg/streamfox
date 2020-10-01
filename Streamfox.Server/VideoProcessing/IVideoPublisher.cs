namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoPublisher
    {
        void PublishVideo(VideoId videoId);
    }
}