namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IFileSystemThumbnailExtractor
    {
        Task ExtractThumbnail(string videoPath, string thumbnailPath);
    }
}