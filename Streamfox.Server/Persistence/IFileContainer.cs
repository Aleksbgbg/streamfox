namespace Streamfox.Server.Persistence
{
    public interface IFileContainer
    {
        string[] ListFiles();

        bool FileExists(string name);
    }
}