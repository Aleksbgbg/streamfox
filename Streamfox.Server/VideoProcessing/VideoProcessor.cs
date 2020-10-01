namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public class VideoProcessor : IVideoProcessor
    {
        private readonly IIntermediateVideoWriter _intermediateVideoWriter;

        private readonly IVideoComponentExistenceChecker _videoComponentExistenceChecker;

        private readonly IMetadataSaver _metadataSaver;

        private readonly IVideoComponentPathResolver _videoComponentPathResolver;

        private readonly IVideoOperationRunner _videoOperationRunner;

        private readonly IVideoConverter _videoConverter;

        public VideoProcessor(
                IIntermediateVideoWriter intermediateVideoWriter,
                IVideoComponentExistenceChecker videoComponentExistenceChecker,
                IMetadataSaver metadataSaver,
                IVideoComponentPathResolver videoComponentPathResolver,
                IVideoOperationRunner videoOperationRunner, IVideoConverter videoConverter)
        {
            _intermediateVideoWriter = intermediateVideoWriter;
            _videoComponentExistenceChecker = videoComponentExistenceChecker;
            _metadataSaver = metadataSaver;
            _videoComponentPathResolver = videoComponentPathResolver;
            _videoOperationRunner = videoOperationRunner;
            _videoConverter = videoConverter;
        }

        public async Task<bool> ProcessVideo(VideoId videoId, Stream videoStream)
        {
            await _intermediateVideoWriter.SaveVideo(videoId, videoStream);

            string sourcePath = _videoComponentPathResolver.ResolveIntermediateVideoPath(videoId);
            string outputPath = _videoComponentPathResolver.ResolveVideoPath(videoId);
            string thumbnailPath = _videoComponentPathResolver.ResolveThumbnailPath(videoId);

            await _videoOperationRunner.ExtractThumbnail(sourcePath, thumbnailPath);

            bool thumbnailExists = _videoComponentExistenceChecker.ThumbnailExists(videoId);

            if (!thumbnailExists)
            {
                _intermediateVideoWriter.DeleteVideo(videoId);
                return false;
            }

            VideoMetadata videoMetadata = await _videoOperationRunner.GrabVideoMetadata(sourcePath);

            if ((videoMetadata.VideoCodec == VideoCodec.Vp9 &&
                 videoMetadata.VideoFormat == VideoFormat.Webm) ||
                (videoMetadata.VideoCodec == VideoCodec.H264 &&
                 videoMetadata.VideoFormat == VideoFormat.Mp4))
            {
                await _videoOperationRunner.NoOpCopy(sourcePath, outputPath);
            }
            else if (videoMetadata.VideoCodec != VideoCodec.Invalid)
            {
                _videoConverter.RunConversionTask(
                        _videoOperationRunner.ConvertToVp9Webm(sourcePath, outputPath));
                videoMetadata = new VideoMetadata(VideoCodec.Vp9, VideoFormat.Webm);
            }

            await _metadataSaver.SaveMetadata(videoId, videoMetadata);

            return true;
        }
    }
}