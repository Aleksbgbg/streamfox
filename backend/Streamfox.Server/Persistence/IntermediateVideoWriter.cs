namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.VideoManagement;
    using Streamfox.Server.VideoProcessing;

    public class IntermediateVideoWriter : IIntermediateVideoWriter, IIntermediateVideoDeleter
    {
        private readonly IFileStreamWriter _fileStreamWriter;

        private readonly IFileDeleter _fileDeleter;

        public IntermediateVideoWriter(IFileStreamWriter fileStreamWriter, IFileDeleter fileDeleter)
        {
            _fileStreamWriter = fileStreamWriter;
            _fileDeleter = fileDeleter;
        }

        public async Task SaveVideo(VideoId videoId, Stream videoStream)
        {
            await _fileStreamWriter.WriteStream(videoId.ToString(), videoStream);
        }

        public void DeleteVideo(VideoId videoId)
        {
            _fileDeleter.Delete(videoId.ToString());
        }

        public Task DeleteIntermediateVideo(VideoId videoId)
        {
            DeleteVideo(videoId);
            return Task.CompletedTask;
        }
    }
}