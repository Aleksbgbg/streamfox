namespace Streamfox.Server.VideoManagement.Persistence
{
    using System.IO;

    public interface IFileSystemManipulator
    {
        Stream OpenFile(string name);
    }
}