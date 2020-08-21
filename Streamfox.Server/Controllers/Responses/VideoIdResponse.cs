namespace Streamfox.Server.Controllers.Responses
{
    using Streamfox.Server.VideoManagement;

    public class VideoIdResponse
    {
        public VideoIdResponse()
        {
        }

        public VideoIdResponse(VideoId videoId)
        {
            VideoId = videoId.ToString();
        }

        public string VideoId { get; set; }
    }
}