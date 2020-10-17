namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    public interface IProgressSink
    {
        Task ReportProgress(ProgressSinkReport report);
    }
}