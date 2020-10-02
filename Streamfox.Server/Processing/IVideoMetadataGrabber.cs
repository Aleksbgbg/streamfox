namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IVideoMetadataGrabber
    {
        Task<VideoMetadata> GrabMetadata(string videoPath);
    }
}