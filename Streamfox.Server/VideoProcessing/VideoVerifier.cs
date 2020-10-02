namespace Streamfox.Server.VideoProcessing
{
    using Streamfox.Server.VideoManagement;

    public class VideoVerifier : IVideoVerifier
    {
        public VideoVerifier()
        {

        }

        public bool IsValidVideo(VideoId videoId)
        {
            return false; 
        }
    }
}