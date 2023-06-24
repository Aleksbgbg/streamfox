namespace Streamfox.Server.Controllers.Responses
{
    using System.Linq;

    using Streamfox.Server.VideoManagement;

    public class VideoListResponse
    {
        public VideoListResponse()
        {
        }

        public VideoListResponse(VideoId[] videoIds)
        {
            VideoIds = videoIds.Select(id => id.Value.ToString()).ToArray();
        }

        public string[] VideoIds { get; set; }
    }
}