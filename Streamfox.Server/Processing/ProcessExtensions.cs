namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class ProcessExtensions
    {
        public static Task<int> StartAsync(this Process process)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) =>
            {
                if (!taskCompletionSource.Task.IsCompleted)
                {
                    taskCompletionSource.SetResult(process.ExitCode);
                    process.Dispose();
                }
            };

            process.Start();

            return taskCompletionSource.Task;
        }
    }
}