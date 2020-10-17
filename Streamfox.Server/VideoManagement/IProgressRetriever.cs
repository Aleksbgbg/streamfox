namespace Streamfox.Server.VideoManagement
{
    using Streamfox.Server.Types;

    public interface IProgressRetriever
    {
        Optional<ConversionProgress> RetrieveConversionProgress(VideoId videoId);
    }
}