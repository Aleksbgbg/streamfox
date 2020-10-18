namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class VideoProcessor : IVideoProcessor
    {
        private readonly IIntermediateVideoWriter _intermediateVideoWriter;

        private readonly IVideoVerifier _videoVerifier;

        private readonly IBackgroundVideoProcessor _backgroundVideoProcessor;

        private readonly ITaskRunner _taskRunner;

        private readonly IFramesFetcher _framesFetcher;

        private readonly IVideoProgressStore _videoProgressStore;

        private readonly IThumbnailExtractor _thumbnailExtractor;

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter, IVideoVerifier videoVerifier,
                IBackgroundVideoProcessor backgroundVideoProcessor, ITaskRunner taskRunner,
                IFramesFetcher framesFetcher, IVideoProgressStore videoProgressStore, IThumbnailExtractor thumbnailExtractor)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _videoVerifier = videoVerifier;
            _backgroundVideoProcessor = backgroundVideoProcessor;
            _taskRunner = taskRunner;
            _framesFetcher = framesFetcher;
            _videoProgressStore = videoProgressStore;
            _thumbnailExtractor = thumbnailExtractor;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            if (await _videoVerifier.IsValidVideo(videoId))
            {
                await _thumbnailExtractor.ExtractThumbnail(videoId);

                await _videoProgressStore.StoreNewVideo(
                        videoId,
                        await _framesFetcher.FetchVideoFrames(videoId));

                _taskRunner.RunBackground(_backgroundVideoProcessor.ProcessVideo(videoId));
                return true;
            }

            _intermediateVideoWriter.DeleteVideo(videoId);
            return false;
        }
    }
}