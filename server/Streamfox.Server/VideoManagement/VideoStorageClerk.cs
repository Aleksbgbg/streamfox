namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public class VideoStorageClerk
    {
        private readonly IVideoIdGenerator _videoIdGenerator;

        private readonly IVideoProcessor _videoProcessor;

        public VideoStorageClerk(IVideoIdGenerator videoIdGenerator, IVideoProcessor videoProcessor)
        {
            _videoIdGenerator = videoIdGenerator;
            _videoProcessor = videoProcessor;
        }

        public async Task<Optional<VideoId>> StoreVideo(Stream videoStream)
        {
            VideoId videoId = _videoIdGenerator.GenerateVideoId();

            bool processSuccessful = await _videoProcessor.ProcessVideo(videoId, videoStream);

            if (processSuccessful)
            {
                return Optional.Of(videoId);
            }

            return Optional<VideoId>.Empty();
        }
    }
}