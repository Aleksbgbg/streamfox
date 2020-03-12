namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Threading.Tasks;

    using IOFile = System.IO.File;

    public class File
    {
        public File(string filename)
        {
            Filename = filename;
        }

        public string Filename { get; }

        public Task<byte[]> ReadBytes()
        {
            return IOFile.ReadAllBytesAsync(Filename);
        }
    }
}