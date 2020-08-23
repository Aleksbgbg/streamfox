namespace Streamfox.Server.Persistence.Operations
{
    using System.Threading.Tasks;

    public interface IFileReader
    {
        Task<string> Read(string name);
    }
}