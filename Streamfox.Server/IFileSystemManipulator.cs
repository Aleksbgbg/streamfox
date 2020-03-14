namespace Streamfox.Server
{
    using System.IO;

    public interface IFileSystemManipulator
    {
        Stream OpenFile(string name);
    }
}