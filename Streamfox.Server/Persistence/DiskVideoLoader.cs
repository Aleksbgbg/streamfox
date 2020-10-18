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

        private readonly IFileExistenceChecker _videoExistenceChecker;

        private readonly IFileReadOpener _videoFileReadOpener;

        private readonly IFileReadOpener _thumbnailFileReadOpener;

        private readonly IFileExistenceChecker _thumbnailExistenceChecker;

        public DiskVideoLoader(
                IFileLister fileLister, IFileExistenceChecker videoExistenceChecker,
                IFileReadOpener videoFileReadOpener, IFileReadOpener thumbnailFileReadOpener,
                IFileExistenceChecker thumbnailExistenceChecker)
        {
            _fileLister = fileLister;
            _videoExistenceChecker = videoExistenceChecker;
            _videoFileReadOpener = videoFileReadOpener;
            _thumbnailFileReadOpener = thumbnailFileReadOpener;
            _thumbnailExistenceChecker = thumbnailExistenceChecker;
        }

        public Optional<Stream> LoadVideo(string label)
        {
            if (_videoExistenceChecker.Exists(label))
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
            if (_thumbnailExistenceChecker.Exists(label))
            {
                return Optional.Of(_thumbnailFileReadOpener.OpenRead(label));
            }

            return Optional<Stream>.Empty();
        }
    }
}