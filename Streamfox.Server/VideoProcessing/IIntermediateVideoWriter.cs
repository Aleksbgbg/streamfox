namespace Streamfox.Server.VideoProcessing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IIntermediateVideoWriter
    {
        Task SaveVideo(VideoId videoId, Stream videoStream);

        void DeleteVideo(VideoId videoId);
    }
}