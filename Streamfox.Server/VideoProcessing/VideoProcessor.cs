namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class VideoProcessor : IVideoProcessor
    {
        private readonly IIntermediateVideoWriter _intermediateVideoWriter;

        private readonly IMultimediaFramework _multimediaFramework;

        private readonly IExistenceChecker _existenceChecker;

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter,
                IMultimediaFramework multimediaFramework, IExistenceChecker existenceChecker)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _multimediaFramework = multimediaFramework;
            _existenceChecker = existenceChecker;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            await _multimediaFramework.ExtractVideoThumbnail(videoId);
            await _multimediaFramework.ExtractVideo(videoId);

            _intermediateVideoWriter.DeleteVideo(videoId);

            return _existenceChecker.ThumbnailExists(videoId) &&
                   _existenceChecker.VideoExists(videoId);
        }
    }
}