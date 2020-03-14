namespace Streamfox.Server
{
    using System;
    using System.IO;

    public class VideoDirectoryHandler : IFileSystemChecker, IFileSystemManipulator
    {
        private const string DirectoryName = "Videos";

        private static readonly string DirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DirectoryName);

        public bool FileExists(string name)
        {
            return File.Exists(PathToFile(name));
        }

        public Stream OpenFile(string name)
        {
            if (FileExists(name))
            {
                return File.OpenRead(PathToFile(name));
            }

            Directory.CreateDirectory(DirectoryPath);

            return File.Create(PathToFile(name));
        }

        private static string PathToFile(string name)
        {
            return Path.Combine(DirectoryPath, name);
        }
    }
}