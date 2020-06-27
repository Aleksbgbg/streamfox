namespace Streamfox.Server.VideoManagement.Processing
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public interface IVideoSnapshotter
    {
        Task<Optional<Stream>> ProduceVideoSnapshot(Stream video);
    }
}