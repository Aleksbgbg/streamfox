namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class MultimediaProcessor : IMultimediaProcessor
    {
        private readonly IPathResolver _pathResolver;

        private readonly IVideoOperationRunner _videoOperationRunner;

        public MultimediaProcessor(IPathResolver pathResolver, IVideoOperationRunner videoOperationRunner)
        {
            _pathResolver = pathResolver;
            _videoOperationRunner = videoOperationRunner;
        }

        public async Task ExtractVideoThumbnail(VideoId videoId)
        {
            string sourceVideoPath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string thumbnailPath = _pathResolver.ResolveThumbnailPath(videoId);

            await _videoOperationRunner.ExtractThumbnail(sourceVideoPath, thumbnailPath);
        }

        public async Task<VideoMetadata> ExtractVideoAndCoerceToSupportedFormats(VideoId videoId)
        {
            string sourcePath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string outputPath = _pathResolver.ResolveVideoPath(videoId);

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
                await _videoOperationRunner.ConvertToVp9Webm(sourcePath, outputPath);
                videoMetadata = new VideoMetadata(VideoCodec.Vp9, VideoFormat.Webm);
            }

            return videoMetadata;
        }
    }
}