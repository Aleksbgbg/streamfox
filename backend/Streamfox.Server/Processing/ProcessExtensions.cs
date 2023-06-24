namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public static class ProcessExtensions
    {
        public static async Task<int> StartAsync(this Process process)
        {
            using (process)
            {
                TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

                process.EnableRaisingEvents = true;
                process.Exited += (sender, e) => taskCompletionSource.SetResult(process.ExitCode);

                process.Start();

                return await taskCompletionSource.Task;
            }
        }
    }
}