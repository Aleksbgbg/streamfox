namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IVideoProgressStore
    {
        Task StoreNewVideo(VideoId videoId, int frames);
    }
}