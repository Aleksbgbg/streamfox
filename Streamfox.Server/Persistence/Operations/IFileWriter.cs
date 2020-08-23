namespace Streamfox.Server.Persistence.Operations
{
    using System.Threading.Tasks;

    public interface IFileWriter
    {
        Task Write(string name, string content);
    }
}