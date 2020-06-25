namespace Streamfox.Server.VideoManagement.Processing
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class VideoSnapshotter : IVideoSnapshotter
    {
        public async Task<Stream> ProduceVideoSnapshot(Stream video)
        {
            video.Position = 0;

            Process process = Process.Start(
                    new ProcessStartInfo("ffmpeg", "-i pipe: -vframes 1 -q:v 2 pipe:.jpg")
                    {
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });

            await using (process.StandardInput.BaseStream)
            {
                await video.CopyToAsync(process.StandardInput.BaseStream);
            }

            MemoryStream outputStream = new MemoryStream();

            await using (process.StandardOutput.BaseStream)
            {
                await process.StandardOutput.BaseStream.CopyToAsync(outputStream);
            }

            process.WaitForExit();

            video.Position = 0;
            outputStream.Position = 0;

            return outputStream;
        }
    }
}