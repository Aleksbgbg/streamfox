namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IFormatConverterWithLogging
    {
        Task<IProgressLogger> CoerceVideoToSupportedFormat(VideoId videoId);
    }
}