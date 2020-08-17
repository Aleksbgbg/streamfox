namespace Streamfox.Server.Persistence
{
    using System;
    using System.IO;

    public class DirectoryHandler : IFileContainer, IFileReader, IFileWriter
    {
        private readonly string _directoryPath;

        public DirectoryHandler(string directoryName)
        {
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);
            Directory.CreateDirectory(_directoryPath);
        }

        public bool FileExists(string name)
        {
            return File.Exists(PathToFile(name));
        }

        public string[] ListFiles()
        {
            return Directory.GetFiles(_directoryPath);
        }

        public Stream OpenRead(string name)
        {
            return File.OpenRead(PathToFile(name));
        }

        public Stream OpenWrite(string name)
        {
            Directory.CreateDirectory(_directoryPath);

            return File.Create(PathToFile(name));
        }

        private string PathToFile(string name)
        {
            return Path.Combine(_directoryPath, name);
        }
    }
}