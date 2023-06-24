namespace Streamfox.Server.Persistence.Operations
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class FileStore : IFileDeleter, IFileExistenceChecker, IFileLister, IFilePathResolver,
                             IFileReader, IFileReadOpener, IFileStreamWriter, IFileWriter
    {
        private readonly string _directoryPath;

        public FileStore(string directoryName)
        {
            _directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryName);
        }

        public void EnsureFileStorePresent()
        {
            Directory.CreateDirectory(_directoryPath);
        }

        public void Delete(string name)
        {
            File.Delete(Resolve(name));
        }

        public bool Exists(string name)
        {
            return File.Exists(Resolve(name));
        }

        public string[] ListFiles()
        {
            return Directory.GetFiles(_directoryPath);
        }

        public Stream OpenRead(string name)
        {
            return File.OpenRead(Resolve(name));
        }

        public async Task<string> Read(string name)
        {
            using StreamReader streamReader = new StreamReader(OpenRead(name));
            return await streamReader.ReadToEndAsync();
        }

        public async Task Write(string name, string content)
        {
            await using StreamWriter streamWriter = new StreamWriter(OpenWrite(name));
            await streamWriter.WriteAsync(content);
        }

        public async Task WriteStream(string name, Stream stream)
        {
            await using Stream fileStream = OpenWrite(name);
            await stream.CopyToAsync(fileStream);
        }

        public string Resolve(string name)
        {
            return Path.Combine(_directoryPath, name);
        }

        private Stream OpenWrite(string name)
        {
            return File.OpenWrite(Resolve(name));
        }
    }
}