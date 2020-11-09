namespace Streamfox.Server.VideoManagement
{
    public interface IVideoExistenceChecker
    {
        bool VideoExists(VideoId videoId);
    }
}