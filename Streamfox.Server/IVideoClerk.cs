namespace Streamfox.Server
{
    using System.IO;

    public interface IVideoClerk
    {
        VideoId StoreVideo(Stream videoStream);

        Optional<Stream> RetrieveVideo(VideoId videoId);
    }
}