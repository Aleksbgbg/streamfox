namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IIntermediateVideoDeleter
    {
        Task DeleteIntermediateVideo(VideoId videoId);
    }
}