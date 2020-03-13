namespace Streamfox.Server
{
    using System.IO;

    public interface IVideoLoader
    {
        Optional<Stream> LoadVideo(string label);
    }
}