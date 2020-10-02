namespace Streamfox.Server.VideoProcessing
{
    using System.Threading.Tasks;

    public interface ITaskRunner
    {
        void RunBackground(Task task);
    }
}