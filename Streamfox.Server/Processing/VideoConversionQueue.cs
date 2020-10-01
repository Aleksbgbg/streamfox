using Streamfox.Server.VideoProcessing;

namespace Streamfox.Server.Processing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public class VideoConversionQueue : IVideoConverter
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();

        public void RunConversionTask(Task task)
        {
            _tasks.Enqueue(task);
            _semaphore.Release();
        }

        public async Task<Task> GetNextConversionTask(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            _tasks.TryDequeue(out Task task);
            return task;
        }
    }
}