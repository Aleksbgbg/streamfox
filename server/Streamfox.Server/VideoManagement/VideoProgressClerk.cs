namespace Streamfox.Server.VideoManagement
{
    using Streamfox.Server.Types;

    public class VideoProgressClerk
    {
        private readonly IProgressRetriever _progressRetriever;

        public VideoProgressClerk(IProgressRetriever progressRetriever)
        {
            _progressRetriever = progressRetriever;
        }

        public Optional<ConversionProgress> RetrieveConversionProgress(VideoId videoId)
        {
            return _progressRetriever.RetrieveProgress(videoId);
        }
    }
}