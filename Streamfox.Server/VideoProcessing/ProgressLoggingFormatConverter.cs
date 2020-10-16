namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public class ProgressLoggingFormatConverter : IFormatConverter
    {
        public Task CoerceVideoToSupportedFormat(VideoId videoId)
        {
            throw new System.NotImplementedException();
        }
    }
}