namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.VideoManagement;

    public interface IIntermediateVideoPathResolver
    {
        string ResolveIntermediateVideoPath(VideoId videoId);
    }
}