namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Linq;

    using Streamfox.Server.Types;

    public class VideoRetrievalClerk
    {
        private readonly IVideoLoader _videoLoader;

        public VideoRetrievalClerk(IVideoLoader videoLoader)
        {
            _videoLoader = videoLoader;
        }

        public Optional<Stream> RetrieveVideo(VideoId videoId)
        {
            return _videoLoader.LoadVideo(videoId.ToString());
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