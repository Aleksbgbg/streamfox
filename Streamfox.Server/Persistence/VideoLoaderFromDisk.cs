namespace Streamfox.Server.Persistence
{
    using System.IO;
    using System.Linq;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    public class VideoLoaderFromDisk : IVideoLoader
    {
        private readonly IFileContainer _videoFileContainer;

        private readonly IVideoFileReader _videoFileReader;

        private readonly IThumbnailFileReader _thumbnailFileReader;

        public VideoLoaderFromDisk(
                IVideoFileContainer videoFileContainer, IVideoFileReader videoFileReader,
                IThumbnailFileReader thumbnailFileReader)
        {
            _videoFileContainer = videoFileContainer;
            _videoFileReader = videoFileReader;
            _thumbnailFileReader = thumbnailFileReader;
        }

        public Optional<Stream> LoadVideo(string label)
        {
            if (_videoFileContainer.FileExists(label))
            {
                return Optional.Of(_videoFileReader.OpenRead(label));
            }

            return Optional<Stream>.Empty();
        }

        public string[] ListLabels()
        {
            return _videoFileContainer.ListFiles().Select(Path.GetFileName).ToArray();
        }

        public Optional<Stream> LoadThumbnail(string label)
        {
            if (_videoFileContainer.FileExists(label))
            {
                return Optional.Of(_thumbnailFileReader.OpenRead(label));
            }

            return Optional<Stream>.Empty();
        }
    }
}