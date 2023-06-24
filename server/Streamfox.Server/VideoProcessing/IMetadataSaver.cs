namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public interface IMetadataSaver
    {
        Task SaveMetadata(VideoId videoId, VideoMetadata videoMetadata);
    }
}