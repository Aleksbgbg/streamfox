namespace Streamfox.Server.VideoManagement
{
    public interface IThumbnailExistenceChecker
    {
        bool ThumbnailExists(VideoId videoId);
    }
}