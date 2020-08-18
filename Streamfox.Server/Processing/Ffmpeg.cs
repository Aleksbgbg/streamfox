namespace Streamfox.Server.Processing
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    public class Ffmpeg : IFfmpeg
    {
        public Task ExtractThumbnail(string videoPath, string thumbnailPath)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo(
                        "ffmpeg",
                        $"-i \"{videoPath}\" -vframes 1 -q:v 5 -vf scale=-1:225 -f singlejpeg \"{thumbnailPath}\"")
            };

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            process.EnableRaisingEvents = true;
            process.Exited += (sender, e) => taskCompletionSource.SetResult(true);

            process.Start();

            return taskCompletionSource.Task;
        }

        public Task NoOp(string sourcePath, string outputPath)
        {
            File.Copy(sourcePath, outputPath);
            return Task.CompletedTask;
        }
    }
}