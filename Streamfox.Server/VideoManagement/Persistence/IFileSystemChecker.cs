namespace Streamfox.Server.VideoManagement.Persistence
{
    public interface IFileSystemChecker
    {
        bool FileExists(string name);

        string[] ListFiles();
    }
}