namespace Streamfox.Server.Persistence.Operations
{
    public interface IFileExistenceChecker
    {
        bool Exists(string name);
    }
}