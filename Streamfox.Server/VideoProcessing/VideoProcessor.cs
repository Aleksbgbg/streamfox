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

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter, IVideoVerifier videoVerifier,
                IBackgroundVideoProcessor backgroundVideoProcessor, ITaskRunner taskRunner)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _videoVerifier = videoVerifier;
            _backgroundVideoProcessor = backgroundVideoProcessor;
            _taskRunner = taskRunner;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            if (_videoVerifier.IsValidVideo(videoId))
            {
                _taskRunner.RunBackground(_backgroundVideoProcessor.ProcessVideo(videoId));
                return true;
            }

            _intermediateVideoWriter.DeleteVideo(videoId);
            return false;
        }
    }
}