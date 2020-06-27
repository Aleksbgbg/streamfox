namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement.Processing;

    public class VideoStorageClerk
    {
        private readonly IVideoIdGenerator _videoIdGenerator;

        private readonly IVideoSaver _videoSaver;

        private readonly IVideoSnapshotter _videoSnapshotter;

        public VideoStorageClerk(
                IVideoIdGenerator videoIdGenerator, IVideoSaver videoSaver,
                IVideoSnapshotter videoSnapshotter)
        {
            _videoIdGenerator = videoIdGenerator;
            _videoSaver = videoSaver;
            _videoSnapshotter = videoSnapshotter;
        }

        public async Task<Optional<VideoId>> StoreVideo(Stream videoStream)
        {
            VideoId videoId = _videoIdGenerator.GenerateVideoId();
            Optional<Stream> snapshot = await _videoSnapshotter.ProduceVideoSnapshot(videoStream);

            if (!snapshot.HasValue)
            {
                return Optional<VideoId>.Empty();
            }

            await _videoSaver.SaveVideo(videoId.ToString(), videoStream, snapshot.Value);

            return Optional.Of(videoId);
        }
    }
}