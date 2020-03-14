namespace Streamfox.Server.VideoManagement
{
    using System.IO;

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
    }
}