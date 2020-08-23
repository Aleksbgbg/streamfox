namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public class VideoProcessor : IVideoProcessor
    {
        private readonly IIntermediateVideoWriter _intermediateVideoWriter;

        private readonly IMultimediaProcessor _multimediaProcessor;

        private readonly IVideoComponentExistenceChecker _videoComponentExistenceChecker;

        private readonly IMetadataSaver _metadataSaver;

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter,
                IMultimediaProcessor multimediaProcessor, IVideoComponentExistenceChecker videoComponentExistenceChecker,
                IMetadataSaver metadataSaver)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _multimediaProcessor = multimediaProcessor;
            _videoComponentExistenceChecker = videoComponentExistenceChecker;
            _metadataSaver = metadataSaver;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            await _multimediaProcessor.ExtractVideoThumbnail(videoId);
            VideoMetadata videoMetadata =
                    await _multimediaProcessor.ExtractVideoAndCoerceToSupportedFormats(videoId);

            bool processingSuccessful = _videoComponentExistenceChecker.ThumbnailExists(videoId) &&
                                        _videoComponentExistenceChecker.VideoExists(videoId);

            if (processingSuccessful)
            {
                await _metadataSaver.SaveMetadata(videoId, videoMetadata);
            }

            _intermediateVideoWriter.DeleteVideo(videoId);

            return processingSuccessful;
        }
    }
}