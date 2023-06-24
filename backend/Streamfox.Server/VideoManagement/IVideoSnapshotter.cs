namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public interface IVideoSnapshotter
    {
        Task<Optional<Stream>> ProduceVideoSnapshot(Stream video);
    }
}