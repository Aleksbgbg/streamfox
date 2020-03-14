namespace Streamfox.Server
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IVideoSaver
    {
        Task SaveVideo(string label, Stream stream);
    }
}