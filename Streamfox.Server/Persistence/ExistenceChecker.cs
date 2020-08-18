namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class ExistenceChecker : IExistenceChecker
    {
        private readonly DirectoryHandler _thumbnailHandler;

        private readonly DirectoryHandler _videoHandler;

        public ExistenceChecker(DirectoryHandler thumbnailHandler, DirectoryHandler videoHandler)
        {
            _thumbnailHandler = thumbnailHandler;
            _videoHandler = videoHandler;
        }

        public bool ThumbnailExists(VideoId videoId)
        {
            return _thumbnailHandler.FileExists(videoId.ToString());
        }

        public bool VideoExists(VideoId videoId)
        {
            return _videoHandler.FileExists(videoId.ToString());
        }
    }
}