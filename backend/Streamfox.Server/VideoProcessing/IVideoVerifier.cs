namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IVideoVerifier
    {
        Task<bool> IsValidVideo(VideoId videoId);
    }
}