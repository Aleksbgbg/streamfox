namespace Streamfox.Server.VideoManagement
{
    using System.IO;

    public interface IVideoLoader
    {
        Stream LoadVideo(VideoId videoId);

        Stream LoadThumbnail(VideoId videoId);

        VideoId[] ListLabels();
    }
}