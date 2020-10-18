namespace Streamfox.Server.Processing.Ffmpeg
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoProcessing;

    public interface IFfmpegProcessRunner
    {
        Task RunFfmpeg(string args);

        Task<IProgressLogger> RunFfmpegWithProgressLogging(string args);

        Task<string> RunFfprobe(string args);
    }
}