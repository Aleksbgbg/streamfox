namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    public interface IProcessLogger
    {
        bool HasAvailableLogs();

        Task<string> RetrieveLog();
    }
}