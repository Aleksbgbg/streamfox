namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public class VideoComponentPathResolverFacade : IVideoComponentPathResolver
    {
        private readonly IPathResolver _intermediateVideoPathResolver;

        private readonly IPathResolver _thumbnailPathResolver;

        private readonly IPathResolver _videoPathResolver;

        public VideoComponentPathResolverFacade(
                IPathResolver intermediateVideoPathResolver, IPathResolver thumbnailPathResolver,
                IPathResolver videoPathResolver)
        {
            _intermediateVideoPathResolver = intermediateVideoPathResolver;
            _thumbnailPathResolver = thumbnailPathResolver;
            _videoPathResolver = videoPathResolver;
        }

        public string ResolveIntermediateVideoPath(VideoId videoId)
        {
            return _intermediateVideoPathResolver.Resolve(videoId.ToString());
        }

        public string ResolveThumbnailPath(VideoId videoId)
        {
            return _thumbnailPathResolver.Resolve(videoId.ToString());
        }

        public string ResolveVideoPath(VideoId videoId)
        {
            return _videoPathResolver.Resolve(videoId.ToString());
        }
    }
}