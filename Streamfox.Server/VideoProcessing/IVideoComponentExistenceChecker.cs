namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoComponentExistenceChecker
    {
        bool ThumbnailExists(VideoId videoId);

        bool VideoExists(VideoId videoId);
    }
}