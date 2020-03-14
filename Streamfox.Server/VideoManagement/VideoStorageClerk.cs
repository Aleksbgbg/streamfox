namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    public class VideoStorageClerk
    {
        private readonly IVideoIdGenerator _videoIdGenerator;

        private readonly IVideoSaver _videoSaver;

        public VideoStorageClerk(IVideoIdGenerator videoIdGenerator, IVideoSaver videoSaver)
        {
            _videoIdGenerator = videoIdGenerator;
            _videoSaver = videoSaver;
        }

        public async Task<VideoId> StoreVideo(Stream videoStream)
        {
            VideoId videoId = _videoIdGenerator.GenerateVideoId();
            await _videoSaver.SaveVideo(videoId.ToString(), videoStream);
            return videoId;
        }
    }
}