namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IFormatConverter
    {
        Task CoerceVideoToSupportedFormat(VideoId videoId);
    }
}