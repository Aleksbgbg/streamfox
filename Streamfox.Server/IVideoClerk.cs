namespace Streamfox.Server
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IVideoClerk
    {
        Task<VideoId> StoreVideo(Stream videoStream);

        Optional<Stream> RetrieveVideo(VideoId videoId);
    }
}