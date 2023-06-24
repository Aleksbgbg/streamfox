namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IThumbnailExtractor
    {
        Task ExtractThumbnail(VideoId videoId);
    }
}