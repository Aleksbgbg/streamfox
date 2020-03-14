namespace Streamfox.Server.VideoManagement
{
    using System.IO;

    using Streamfox.Server.Types;

    public interface IVideoLoader
    {
        Optional<Stream> LoadVideo(string label);
    }
}