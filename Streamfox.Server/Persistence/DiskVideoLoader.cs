namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Linq;

    using Streamfox.Server.Persistence.Operations;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    public class DiskVideoLoader : IVideoLoader
    {
        private readonly IFileLister _fileLister;

        private readonly IFileExistenceChecker _fileExistenceChecker;

        private readonly IFileReadOpener _videoFileReadOpener;

        private readonly IFileReadOpener _thumbnailFileReadOpener;

        public DiskVideoLoader(
                IFileLister fileLister, IFileExistenceChecker fileExistenceChecker,
                IFileReadOpener videoFileReadOpener, IFileReadOpener thumbnailFileReadOpener)
        {
            _fileLister = fileLister;
            _fileExistenceChecker = fileExistenceChecker;
            _videoFileReadOpener = videoFileReadOpener;
            _thumbnailFileReadOpener = thumbnailFileReadOpener;
        }

        public Optional<Stream> LoadVideo(string label)
        {
            if (_fileExistenceChecker.Exists(label))
            {
                return Optional.Of(_videoFileReadOpener.OpenRead(label));
            }

            return Optional<Stream>.Empty();
        }

        public string[] ListLabels()
        {
            return _fileLister.ListFiles().Select(Path.GetFileName).ToArray();
        }

        public Optional<Stream> LoadThumbnail(string label)
        {
            if (_fileExistenceChecker.Exists(label))
            {
                return Optional.Of(_thumbnailFileReadOpener.OpenRead(label));
            }

            return Optional<Stream>.Empty();
        }
    }
}