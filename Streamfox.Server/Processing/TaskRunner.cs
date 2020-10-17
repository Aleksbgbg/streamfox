namespace Streamfox.Server.Processing
{
    using System.Threading.Tasks;

    using Streamfox.Server.VideoProcessing;

    public class TaskRunner : ITaskRunner
    {
        public void RunBackground(Task task)
        {
            Task.Factory.StartNew(async () => await task, TaskCreationOptions.LongRunning);
        }
    }
}