namespace Streamfox.Server.Processing
{
    using Streamfox.Server.VideoManagement;

    public interface IVideoComponentPathResolver
    {
        string ResolveIntermediateVideoPath(VideoId videoId);

        string ResolveThumbnailPath(VideoId videoId);

        string ResolveVideoPath(VideoId videoId);
    }
}