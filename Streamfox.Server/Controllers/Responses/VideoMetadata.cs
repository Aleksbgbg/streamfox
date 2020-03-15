namespace Streamfox.Server.Controllers.Responses
{
    using Streamfox.Server.VideoManagement;

    public class VideoMetadata
    {
        public VideoMetadata()
        {
        }

        public VideoMetadata(VideoId videoId)
        {
            VideoId = videoId.ToString();
        }

        public string VideoId { get; set; }
    }
}