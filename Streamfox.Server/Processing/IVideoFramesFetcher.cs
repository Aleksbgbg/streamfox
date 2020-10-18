namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IVideoFramesFetcher
    {
        Task<int> FetchVideoFrames(string videoPath);
    }
}