namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoVerifier
    {
        bool IsValidVideo(VideoId videoId);
    }
}