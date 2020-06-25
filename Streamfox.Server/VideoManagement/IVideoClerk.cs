namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public interface IVideoClerk
    {
        Task<VideoId> StoreVideo(Stream videoStream);

        Optional<Stream> RetrieveVideo(VideoId videoId);

        VideoId[] ListVideos();

        Optional<Stream> RetrieveThumbnail(VideoId videoId);
    }
}