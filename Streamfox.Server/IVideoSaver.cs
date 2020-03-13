namespace Streamfox.Server
{
    using System.IO;

    public interface IVideoSaver
    {
        void SaveVideo(string label, Stream stream);
    }
}