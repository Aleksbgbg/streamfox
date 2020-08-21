namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    public interface IProcessRunner
    {
        Task RunFfmpeg(string args);

        Task<string> RunFfprobe(string args);
    }
}