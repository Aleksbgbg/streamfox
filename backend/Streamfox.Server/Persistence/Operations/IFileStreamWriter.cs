namespace Streamfox.Server.Persistence.Operations
{
    using System.IO;
    using System.Threading.Tasks;

    public interface IFileStreamWriter
    {
        Task WriteStream(string name, Stream stream);
    }
}