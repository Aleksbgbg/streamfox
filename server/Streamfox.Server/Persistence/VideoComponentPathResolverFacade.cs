namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.VideoManagement;

    public class VideoComponentPathResolverFacade : IIntermediateVideoPathResolver, IThumbnailPathResolver, IVideoPathResolver
    {
        private readonly IFilePathResolver _intermediateVideoFilePathResolver;

        private readonly IFilePathResolver _thumbnailFilePathResolver;

        private readonly IFilePathResolver _videoFilePathResolver;

        public VideoComponentPathResolverFacade(
                IFilePathResolver intermediateVideoFilePathResolver, IFilePathResolver thumbnailFilePathResolver,
                IFilePathResolver videoFilePathResolver)
        {
            _intermediateVideoFilePathResolver = intermediateVideoFilePathResolver;
            _thumbnailFilePathResolver = thumbnailFilePathResolver;
            _videoFilePathResolver = videoFilePathResolver;
        }

        public string ResolveIntermediateVideoPath(VideoId videoId)
        {
            return _intermediateVideoFilePathResolver.Resolve(videoId.ToString());
        }

        public string ResolveThumbnailPath(VideoId videoId)
        {
            return _thumbnailFilePathResolver.Resolve(videoId.ToString());
        }

        public string ResolveVideoPath(VideoId videoId)
        {
            return _videoFilePathResolver.Resolve(videoId.ToString());
        }
    }
}