namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IVideoSaver
    {
        Task SaveVideo(string label, Stream video, Stream thumbnail);
    }
}