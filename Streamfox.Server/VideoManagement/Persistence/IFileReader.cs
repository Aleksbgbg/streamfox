namespace Streamfox.Server.VideoManagement.Persistence
{
    using System.IO;

    public interface IFileReader
    {
        public Stream OpenRead(string name);
    }
}