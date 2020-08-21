namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.Types;

    public class VideoRetrievalClerk
    {
        private readonly IVideoLoader _videoLoader;

        private readonly IMetadataRetriever _metadataRetriever;

        public VideoRetrievalClerk(IVideoLoader videoLoader, IMetadataRetriever metadataRetriever)
        {
            _videoLoader = videoLoader;
            _metadataRetriever = metadataRetriever;
        }

        public async Task<Optional<StoredVideo>> RetrieveVideo(VideoId videoId)
        {
            Optional<Stream> video = _videoLoader.LoadVideo(videoId.ToString());

            if (video.HasValue)
            {
                VideoMetadata metadata = await _metadataRetriever.RetrieveMetadata(videoId);
                return Optional.Of(new StoredVideo(metadata, video.Value));
            }

            return Optional<StoredVideo>.Empty();
        }

        public VideoId[] ListVideos()
        {
            return _videoLoader.ListLabels()
                               .Select(long.Parse)
                               .Select(id => new VideoId(id))
                               .ToArray();
        }

        public Optional<Stream> RetrieveThumbnail(VideoId videoId)
        {
            return _videoLoader.LoadThumbnail(videoId.ToString());
        }
    }
}