namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;

    public interface IVideoOperationRunner
    {
        public Task ExtractThumbnail(string videoPath, string thumbnailPath);

        public Task<VideoMetadata> GrabVideoMetadata(string videoPath);

        public Task ConvertToVp9Webm(string sourcePath, string outputPath);

        public Task NoOpCopy(string sourcePath, string outputPath);
    }
}