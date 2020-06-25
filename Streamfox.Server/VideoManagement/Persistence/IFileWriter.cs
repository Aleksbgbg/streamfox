namespace Streamfox.Server.VideoManagement.Persistence
{
    using System.IO;

    public interface IFileWriter
    {
        public Stream OpenWrite(string name);
    }
}