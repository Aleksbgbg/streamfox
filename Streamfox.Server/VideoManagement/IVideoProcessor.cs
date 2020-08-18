namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IVideoProcessor
    {
        Task<bool> ProcessVideo(VideoId videoId, Stream videoStream);
    }
}