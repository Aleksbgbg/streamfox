namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IFramesFetcher
    {
        Task<int> FetchVideoFrames(string videoPath);
    }
}