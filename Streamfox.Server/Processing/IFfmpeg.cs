namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IFfmpeg
    {
        public Task ExtractThumbnail(string videoPath, string thumbnailPath);

        public Task<VideoMetadata> GrabVideoMetadata(string videoPath);

        public Task ConvertToVp9Webm(string sourcePath, string outputPath);

        public Task ConvertToMp4(string sourcePath, string outputPath);

        public Task NoOpCopy(string sourcePath, string outputPath);
    }
}