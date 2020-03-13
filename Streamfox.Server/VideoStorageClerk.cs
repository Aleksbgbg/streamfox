namespace Streamfox.Server
{
    using System.IO;

    public class VideoStorageClerk
    {
        private readonly IVideoIdGenerator _videoIdGenerator;

        private readonly IVideoSaver _videoSaver;

        public VideoStorageClerk(IVideoIdGenerator videoIdGenerator, IVideoSaver videoSaver)
        {
            _videoIdGenerator = videoIdGenerator;
            _videoSaver = videoSaver;
        }

        public VideoId StoreVideo(Stream videoStream)
        {
            VideoId videoId = _videoIdGenerator.GenerateVideoId();
            _videoSaver.SaveVideo(videoId.ToString(), videoStream);
            return videoId;
        }
    }
}