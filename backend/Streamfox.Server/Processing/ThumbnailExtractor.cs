namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class ThumbnailExtractor : IThumbnailExtractor
    {
        private readonly IIntermediateVideoPathResolver _intermediateVideoPathResolver;

        private readonly IThumbnailPathResolver _thumbnailPathResolver;

        private readonly IFileSystemThumbnailExtractor _fileSystemThumbnailExtractor;

        public ThumbnailExtractor(
                IIntermediateVideoPathResolver intermediateVideoPathResolver,
                IThumbnailPathResolver thumbnailPathResolver,
                IFileSystemThumbnailExtractor fileSystemThumbnailExtractor)
        {
            _intermediateVideoPathResolver = intermediateVideoPathResolver;
            _thumbnailPathResolver = thumbnailPathResolver;
            _fileSystemThumbnailExtractor = fileSystemThumbnailExtractor;
        }

        public async Task ExtractThumbnail(VideoId videoId)
        {
            string videoPath = _intermediateVideoPathResolver.ResolveIntermediateVideoPath(videoId);
            string thumbnailPath = _thumbnailPathResolver.ResolveThumbnailPath(videoId);

            await _fileSystemThumbnailExtractor.ExtractThumbnail(videoPath, thumbnailPath);
        }
    }
}