namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoProgressStore
    {
        void RegisterVideo(VideoId videoId, int totalFrames);
    }
}