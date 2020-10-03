namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class VideoFinalizer : IVideoFinalizer
    {
        private readonly IIntermediateVideoDeleter _intermediateVideoDeleter;

        public VideoFinalizer(IIntermediateVideoDeleter intermediateVideoDeleter)
        {
            _intermediateVideoDeleter = intermediateVideoDeleter;
        }

        public Task FinalizeVideoProcessing(VideoId videoId)
        {
            return _intermediateVideoDeleter.DeleteIntermediateVideo(videoId);
        }
    }
}