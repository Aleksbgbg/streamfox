namespace Streamfox.Server.VideoManagement
{
    using System.IO;

    using Streamfox.Server.Processing;

    public class StoredVideo
    {
        public StoredVideo(VideoMetadata videoMetadata, Stream videoStream)
        {
            VideoMetadata = videoMetadata;
            VideoStream = videoStream;
        }

        public VideoMetadata VideoMetadata { get; }

        public Stream VideoStream { get; }
    }
}