namespace Streamfox.Server.VideoManagement
{
    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.Types;

    public class VideoProgressClerk
    {
        public Optional<ConversionProgressResponse> RetrieveConversionProgress(VideoId videoId)
        {
            return Optional.Of(new ConversionProgressResponse());
        }
    }
}