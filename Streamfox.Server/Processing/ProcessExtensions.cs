namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public static class ProcessExtensions
    {
        public static Task<int> RunAsTask(this Process process, int timeout = -1)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

            if (timeout > 0)
            {
                CancellationTokenSource cancellationTokenSource =
                        new CancellationTokenSource(timeout);
                cancellationTokenSource.Token.Register(
                        () =>
                        {
                            if (!taskCompletionSource.Task.IsCompleted)
                            {
                                taskCompletionSource.SetResult(-1);
                                process.Dispose();
                            }
                        });
            }

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