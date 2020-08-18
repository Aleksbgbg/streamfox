namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class PathResolver : IPathResolver
    {
        private readonly DirectoryHandler _intermediateHandler;

        private readonly DirectoryHandler _thumbnailHandler;

        private readonly DirectoryHandler _videoHandler;

        public PathResolver(
                DirectoryHandler intermediateHandler, DirectoryHandler thumbnailHandler,
                DirectoryHandler videoHandler)
        {
            _intermediateHandler = intermediateHandler;
            _thumbnailHandler = thumbnailHandler;
            _videoHandler = videoHandler;
        }

        public string ResolveIntermediateVideoPath(VideoId videoId)
        {
            return _intermediateHandler.PathToFile(videoId.ToString());
        }

        public string ResolveThumbnailPath(VideoId videoId)
        {
            return _thumbnailHandler.PathToFile(videoId.ToString());
        }

        public string ResolveVideoPath(VideoId videoId)
        {
            return _videoHandler.PathToFile(videoId.ToString());
        }
    }
}