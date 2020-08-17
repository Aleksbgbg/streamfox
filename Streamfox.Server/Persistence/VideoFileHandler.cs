namespace Streamfox.Server.Persistence
{
    using System.IO;

    public class VideoFileHandler : IVideoFileContainer, IVideoFileReader, IVideoFileWriter
    {
        private readonly DirectoryHandler _directoryHandler;

        public VideoFileHandler(DirectoryHandler directoryHandler)
        {
            _directoryHandler = directoryHandler;
        }

        public bool FileExists(string name)
        {
            return _directoryHandler.FileExists(name);
        }

        public string[] ListFiles()
        {
            return _directoryHandler.ListFiles();
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