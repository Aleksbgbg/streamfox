namespace Streamfox.Server.VideoManagement.Persistence
{
    using System.IO;

    using Streamfox.Server.Types;

    public class VideoLoaderFromDisk : IVideoLoader
    {
        private readonly IFileSystemChecker _fileSystemChecker;

        private readonly IFileSystemManipulator _fileSystemManipulator;

        public VideoLoaderFromDisk(IFileSystemChecker fileSystemChecker, IFileSystemManipulator fileSystemManipulator)
        {
            _fileSystemChecker = fileSystemChecker;
            _fileSystemManipulator = fileSystemManipulator;
        }

        public Optional<Stream> LoadVideo(string label)
        {
            if (_fileSystemChecker.FileExists(label))
            {
                return Optional<Stream>.Of(_fileSystemManipulator.OpenFile(label));
            }

            return Optional<Stream>.Empty();
        }
    }
}