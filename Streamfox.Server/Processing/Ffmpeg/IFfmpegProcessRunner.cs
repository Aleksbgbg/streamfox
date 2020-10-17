namespace Streamfox.Server.Processing.Ffmpeg
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoProcessing;

    public interface IFfmpegProcessRunner
    {
        Task<IProgressLogger> RunFfmpeg(string args);

        Task<string> RunFfprobe(string args);
    }
}