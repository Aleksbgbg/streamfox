namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IVideoFinalizer
    {
        Task FinalizeVideoProcessing(VideoId videoId);
    }
}