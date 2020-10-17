namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    using Streamfox.Server.Processing.Ffmpeg;

    public interface IProgressLogger
    {
        Task<bool> HasMoreProgress();

        Task<ProgressReport> GetNextProgress();
    }
}