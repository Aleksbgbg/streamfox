namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public interface IPathResolver
    {
        string ResolveIntermediateVideoPath(VideoId videoId);

        string ResolveThumbnailPath(VideoId videoId);

        string ResolveVideoPath(VideoId videoId);
    }
}