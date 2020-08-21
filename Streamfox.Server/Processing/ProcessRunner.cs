namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ProcessRunner : IProcessRunner
    {
        public Task RunFfmpeg(string args)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo("ffmpeg", args),
                EnableRaisingEvents = true
            }.RunAsTask();
        }

        public async Task<string> RunFfprobe(string args)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo("ffprobe", args)
                {
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };

            await process.RunAsTask(1000);

            using (process.StandardOutput)
            {
                return await process.StandardOutput.ReadToEndAsync();
            }
        }
    }
}