namespace Streamfox.Server.VideoManagement
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;

    public interface IMetadataRetriever
    {
        Task<VideoMetadata> RetrieveMetadata(VideoId videoId);
    }
}