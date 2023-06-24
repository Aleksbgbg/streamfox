namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IFramesFetcher
    {
        Task<int> FetchVideoFrames(VideoId videoId);
    }
}