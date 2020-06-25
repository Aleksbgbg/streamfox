namespace Streamfox.Server.VideoManagement.Persistence
{
    public interface IFileContainer
    {
        string[] ListFiles();

        bool FileExists(string name);
    }
}