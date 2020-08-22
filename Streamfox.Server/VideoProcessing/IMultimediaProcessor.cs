namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing;
    using Streamfox.Server.VideoManagement;

    public interface IMultimediaProcessor
    {
        Task ExtractVideoThumbnail(VideoId videoId);

        Task<VideoMetadata> ExtractVideoAndCoerceToSupportedFormats(VideoId videoId);
    }
}