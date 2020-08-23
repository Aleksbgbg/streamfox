namespace Streamfox.Server.Persistence.Operations
{
    using System.IO;

    public interface IFileReadOpener
    {
        Stream OpenRead(string name);
    }
}