namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IFfmpeg
    {
        public Task ExtractThumbnail(string videoPath, string thumbnailPath);

        public Task NoOp(string sourcePath, string outputPath);
    }
}