namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.Types;

    public class VideoRetrievalClerk
    {
        private readonly IVideoLoader _videoLoader;

        private readonly IMetadataRetriever _metadataRetriever;

        private readonly IVideoExistenceChecker _videoExistenceChecker;

        private readonly IThumbnailExistenceChecker _thumbnailExistenceChecker;

        private readonly IVideoLister _videoLister;

        public VideoRetrievalClerk(
                IVideoLoader videoLoader, IMetadataRetriever metadataRetriever,
                IVideoExistenceChecker videoExistenceChecker,
                IThumbnailExistenceChecker thumbnailExistenceChecker, IVideoLister videoLister)
        {
            _videoLoader = videoLoader;
            _metadataRetriever = metadataRetriever;
            _videoExistenceChecker = videoExistenceChecker;
            _thumbnailExistenceChecker = thumbnailExistenceChecker;
            _videoLister = videoLister;
        }

        public async Task<Optional<StoredVideo>> RetrieveVideo(VideoId videoId)
        {
            if (_videoExistenceChecker.VideoExists(videoId))
            {
                Stream video = _videoLoader.LoadVideo(videoId);
                VideoMetadata metadata = await _metadataRetriever.RetrieveMetadata(videoId);
                return Optional.Of(new StoredVideo(metadata, video));
            }

            return Optional<StoredVideo>.Empty();
        }

        public VideoId[] ListVideos()
        {
            return _videoLister.ListLabels();
        }

        public Optional<Stream> RetrieveThumbnail(VideoId videoId)
        {
            if (_thumbnailExistenceChecker.ThumbnailExists(videoId))
            {
                return Optional.Of(_videoLoader.LoadThumbnail(videoId));
            }

            return Optional<Stream>.Empty();
        }
    }
}