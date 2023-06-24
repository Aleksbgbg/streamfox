namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IMetadataExtractor
    {
        Task ExtractVideoMetadata(VideoId videoId);
    }
}