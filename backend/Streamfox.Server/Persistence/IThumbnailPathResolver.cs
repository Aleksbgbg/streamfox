namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.VideoManagement;

    public interface IThumbnailPathResolver
    {
        string ResolveThumbnailPath(VideoId videoId);
    }
}