namespace Streamfox.Server
{
    using System.IO;
    using System.Threading.Tasks;

    public class VideoSaverToDisk : IVideoSaver
    {
        private readonly IFileSystemManipulator _fileSystemManipulator;

        public VideoSaverToDisk(IFileSystemManipulator fileSystemManipulator)
        {
            _fileSystemManipulator = fileSystemManipulator;
        }

        public async Task SaveVideo(string label, Stream stream)
        {
            using (Stream fileStream = _fileSystemManipulator.OpenFile(label))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}