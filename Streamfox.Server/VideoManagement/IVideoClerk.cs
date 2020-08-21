namespace Streamfox.Server.VideoManagement
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;

    public interface IVideoClerk
    {
        Task<Optional<VideoId>> StoreVideo(Stream videoStream);

        Task<Optional<StoredVideo>> RetrieveVideo(VideoId videoId);

        VideoId[] ListVideos();

        Optional<Stream> RetrieveThumbnail(VideoId videoId);
    }
}