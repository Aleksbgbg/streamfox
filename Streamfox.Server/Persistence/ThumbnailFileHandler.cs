namespace Streamfox.Server.Persistence
{
    using System.IO;

    public class ThumbnailFileHandler : IThumbnailFileReader, IThumbnailFileWriter
    {
        private readonly DirectoryHandler _directoryHandler;

        public ThumbnailFileHandler(DirectoryHandler directoryHandler)
        {
            _directoryHandler = directoryHandler;
        }

        public Stream OpenRead(string name)
        {
            return _directoryHandler.OpenRead(name);
        }

        public Stream OpenWrite(string name)
        {
            return _directoryHandler.OpenWrite(name);
        }
    }
}