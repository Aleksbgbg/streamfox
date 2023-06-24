namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IBackgroundVideoProcessor
    {
        Task ProcessVideo(VideoId videoId);
    }
}