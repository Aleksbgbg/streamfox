namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class VideoProgressStore : IVideoProgressStore, IProgressSink
    {
        public Task StoreNewVideo(VideoId videoId, int frames)
        {
            return Task.CompletedTask;
        }

        public Task ReportProgress(ProgressSinkReport report)
        {
            return Task.CompletedTask;
        }
    }
}