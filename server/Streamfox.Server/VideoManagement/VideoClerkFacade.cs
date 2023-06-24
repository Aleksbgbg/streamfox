namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public class VideoClerkFacade : IVideoClerk
    {
        private readonly VideoRetrievalClerk _videoRetrievalClerk;

        private readonly VideoStorageClerk _videoStorageClerk;

        private readonly VideoProgressClerk _videoProgressClerk;

        public VideoClerkFacade(
                VideoRetrievalClerk videoRetrievalClerk, VideoStorageClerk videoStorageClerk,
                VideoProgressClerk videoProgressClerk)
        {
            _videoRetrievalClerk = videoRetrievalClerk;
            _videoStorageClerk = videoStorageClerk;
            _videoProgressClerk = videoProgressClerk;
        }

        public Task<Optional<StoredVideo>> RetrieveVideo(VideoId videoId)
        {
            return _videoRetrievalClerk.RetrieveVideo(videoId);
        }

        public VideoId[] ListVideos()
        {
            return _videoRetrievalClerk.ListVideos();
        }

        public Optional<Stream> RetrieveThumbnail(VideoId videoId)
        {
            return _videoRetrievalClerk.RetrieveThumbnail(videoId);
        }

        public Optional<ConversionProgress> RetrieveConversionProgress(VideoId videoId)
        {
            return _videoProgressClerk.RetrieveConversionProgress(videoId);
        }

        public Task<Optional<VideoId>> StoreVideo(Stream videoStream)
        {
            return _videoStorageClerk.StoreVideo(videoStream);
        }
    }
}