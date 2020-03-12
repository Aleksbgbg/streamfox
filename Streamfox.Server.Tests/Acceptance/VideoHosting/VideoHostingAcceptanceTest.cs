namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Xunit;

    public class VideoHostingAcceptanceTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly VideoHost _videoHost;

        public VideoHostingAcceptanceTest(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _videoHost = new VideoHost(webApplicationFactory);
        }

        [Fact]
        public async Task UploadVideo_ThenDownload_ExpectExactCopyDownloaded()
        {
            byte[] readVideoBytes = await new File("Acceptance/VideoHosting/Video.mp4").ReadBytes();

            VideoId videoId = await _videoHost.Upload(readVideoBytes);
            byte[] downloadedVideoBytes = await _videoHost.Download(videoId);

            Assert.Equal(readVideoBytes, downloadedVideoBytes);
        }
    }
}