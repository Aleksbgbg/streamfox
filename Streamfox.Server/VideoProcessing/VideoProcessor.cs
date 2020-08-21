namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public class VideoProcessor : IVideoProcessor
    {
        private readonly IIntermediateVideoWriter _intermediateVideoWriter;

        private readonly IMultimediaFramework _multimediaFramework;

        private readonly IExistenceChecker _existenceChecker;

        private readonly IMetadataSaver _metadataSaver;

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter,
                IMultimediaFramework multimediaFramework, IExistenceChecker existenceChecker,
                IMetadataSaver metadataSaver)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _multimediaFramework = multimediaFramework;
            _existenceChecker = existenceChecker;
            _metadataSaver = metadataSaver;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            await _multimediaFramework.ExtractVideoThumbnail(videoId);
            VideoMetadata videoMetadata =
                    await _multimediaFramework.ExtractVideoAndCoerceToSupportedFormats(videoId);
            await _metadataSaver.SaveMetadata(videoId, videoMetadata);

            _intermediateVideoWriter.DeleteVideo(videoId);

            return _existenceChecker.ThumbnailExists(videoId) &&
                   _existenceChecker.VideoExists(videoId);
        }
    }
}