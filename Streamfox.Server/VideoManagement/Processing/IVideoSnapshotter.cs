namespace Streamfox.Server.VideoManagement.Processing
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IVideoSnapshotter
    {
        Task<Stream> ProduceVideoSnapshot(Stream video);
    }
}