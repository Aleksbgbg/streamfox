namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;

    public interface IMultimediaFramework
    {
        Task ExtractVideoThumbnail(VideoId videoId);

        Task ExtractVideoAndCoerceToSupportedFormats(VideoId videoId);
    }
}