namespace Streamfox.Server.Controllers.Responses
{
    using System.Linq;

    using Streamfox.Server.VideoManagement;

    public class VideoList
    {
        public VideoList()
        {
        }

        public VideoList(VideoId[] videoIds)
        {
            VideoIds = videoIds.Select(id => id.Value.ToString()).ToArray();
        }

        public string[] VideoIds { get; set; }
    }
}