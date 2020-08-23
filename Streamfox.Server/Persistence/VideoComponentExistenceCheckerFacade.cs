namespace Streamfox.Server.Persistence
{
    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class VideoComponentExistenceCheckerFacade : IVideoComponentExistenceChecker
    {
        private readonly IFileExistenceChecker _videoFileExistenceChecker;

        private readonly IFileExistenceChecker _thumbnailFileExistenceChecker;

        public VideoComponentExistenceCheckerFacade(
                IFileExistenceChecker videoFileExistenceChecker,
                IFileExistenceChecker thumbnailFileExistenceChecker)
        {
            _videoFileExistenceChecker = videoFileExistenceChecker;
            _thumbnailFileExistenceChecker = thumbnailFileExistenceChecker;
        }

        public bool VideoExists(VideoId videoId)
        {
            return _videoFileExistenceChecker.Exists(videoId.ToString());
        }

        public bool ThumbnailExists(VideoId videoId)
        {
            return _thumbnailFileExistenceChecker.Exists(videoId.ToString());
        }
    }
}