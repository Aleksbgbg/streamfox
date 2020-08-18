namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IExistenceChecker
    {
        bool ThumbnailExists(VideoId videoId);

        bool VideoExists(VideoId videoId);
    }
}