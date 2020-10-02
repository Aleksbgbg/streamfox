namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoPathResolver
    {
        string ResolveVideoPath(VideoId videoId);
    }
}