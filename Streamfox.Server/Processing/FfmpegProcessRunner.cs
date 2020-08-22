namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class FfmpegProcessRunner : IFfmpegProcessRunner
    {
        public Task RunFfmpeg(string args)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo("ffmpeg", args),
                EnableRaisingEvents = true
            }.StartAsync();
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

            MemoryStream output = new MemoryStream();

            Task runProcess = process.StartAsync();
            Task readOutput = Task.Run(
                    async () =>
                    {
                        using (process.StandardOutput)
                        {
                            await process.StandardOutput.BaseStream.CopyToAsync(output);
                        }
                    });

            await Task.WhenAll(runProcess, readOutput);

            output.Position = 0;
            using StreamReader outputReader = new StreamReader(output);
            return await outputReader.ReadToEndAsync();
        }
    }
}