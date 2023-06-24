namespace Streamfox.Server.Persistence.Operations
{
    public interface IFilePathResolver
    {
        string Resolve(string name);
    }
}