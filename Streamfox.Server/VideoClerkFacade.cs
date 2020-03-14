namespace Streamfox.Server
{
    using System.IO;

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

        public VideoId StoreVideo(Stream videoStream)
        {
            return _videoStorageClerk.StoreVideo(videoStream);
        }
    }
}