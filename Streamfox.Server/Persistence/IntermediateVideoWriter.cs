namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class IntermediateVideoWriter : IIntermediateVideoWriter
    {
        private readonly DirectoryHandler _intermediateHandler;

        public IntermediateVideoWriter(DirectoryHandler intermediateHandler)
        {
            _intermediateHandler = intermediateHandler;
        }

        public async Task SaveVideo(VideoId videoId, Stream videoStream)
        {
            await using Stream fileStream = _intermediateHandler.OpenWrite(videoId.ToString());
            await videoStream.CopyToAsync(fileStream);
        }

        public void DeleteVideo(VideoId videoId)
        {
            _intermediateHandler.Delete(videoId.ToString());
        }
    }
}