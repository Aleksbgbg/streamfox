namespace Streamfox.Server.Processing
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Hosting;

    public class BackgroundVideoConverter : BackgroundService
    {
        private readonly VideoConversionQueue _videoConversionQueue;

        public BackgroundVideoConverter(VideoConversionQueue videoConversionQueue)
        {
            _videoConversionQueue = videoConversionQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Task conversionTask = await _videoConversionQueue.GetNextConversionTask(stoppingToken);
                Task _ = Task.Run(async () => await conversionTask, stoppingToken);
            }
        }
    }
}