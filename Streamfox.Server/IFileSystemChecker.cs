namespace Streamfox.Server
{
    public interface IFileSystemChecker
    {
        bool FileExists(string name);
    }
}