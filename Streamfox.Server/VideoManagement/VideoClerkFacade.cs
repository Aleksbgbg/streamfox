namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public class VideoClerkFacade : IVideoClerk
    {
        private readonly VideoRetrievalClerk _videoRetrievalClerk;

        private readonly VideoStorageClerk _videoStorageClerk;

        public VideoClerkFacade(VideoRetrievalClerk videoRetrievalClerk, VideoStorageClerk videoStorageClerk)
        {
            _videoRetrievalClerk = videoRetrievalClerk;
            _videoStorageClerk = videoStorageClerk;
        }

        public Optional<Stream> RetrieveVideo(VideoId videoId)
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

        public Task<VideoId> StoreVideo(Stream videoStream)
        {
            return _videoStorageClerk.StoreVideo(videoStream);
        }
    }
}