namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class MultimediaFramework : IMultimediaFramework
    {
        private readonly IPathResolver _pathResolver;

        private readonly IFfmpeg _ffmpeg;

        public MultimediaFramework(IPathResolver pathResolver, IFfmpeg ffmpeg)
        {
            _pathResolver = pathResolver;
            _ffmpeg = ffmpeg;
        }

        public async Task ExtractVideoThumbnail(VideoId videoId)
        {
            string sourceVideoPath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string thumbnailPath = _pathResolver.ResolveThumbnailPath(videoId);

            await _ffmpeg.ExtractThumbnail(sourceVideoPath, thumbnailPath);
        }

        public async Task ExtractVideo(VideoId videoId)
        {
            string sourcePath = _pathResolver.ResolveIntermediateVideoPath(videoId);
            string outputPath = _pathResolver.ResolveVideoPath(videoId);

            await _ffmpeg.NoOp(sourcePath, outputPath);
        }
    }
}