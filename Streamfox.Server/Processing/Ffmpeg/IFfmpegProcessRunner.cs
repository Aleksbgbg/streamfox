namespace Streamfox.Server.Processing.Ffmpeg
{
    using System.Threading.Tasks;

    public interface IFfmpegProcessRunner
    {
        Task RunFfmpeg(string args);

        Task<string> RunFfprobe(string args);
    }
}