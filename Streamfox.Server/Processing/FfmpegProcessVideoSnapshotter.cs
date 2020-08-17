namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    public class FfmpegProcessVideoSnapshotter : IVideoSnapshotter
    {
        public async Task<Optional<Stream>> ProduceVideoSnapshot(Stream video)
        {
            video.Position = 0;

            MemoryStream outputStream = new MemoryStream();

            Process process = Process.Start(
                    new ProcessStartInfo("ffmpeg", "-i pipe: -vframes 1 -q:v 2 pipe:.jpg")
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });

            Task inputTask = Task.Run(
                    async () =>
                    {
                        await using (process.StandardInput.BaseStream)
                        {
                            try
                            {
                                await video.CopyToAsync(process.StandardInput.BaseStream);
                            }
                            catch (IOException)
                            {
                            }
                        }
                    });

            Task outputTask = Task.Run(
                    async () =>
                    {
                        await using (process.StandardOutput.BaseStream)
                        {
                            await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
                        }
                    });

            await Task.WhenAll(inputTask, outputTask);

            process.WaitForExit();

            video.Position = 0;
            outputStream.Position = 0;

            if (outputStream.Length == 0)
            {
                return Optional<Stream>.Empty();
            }

            return Optional.Of((Stream)outputStream);
        }
    }
}