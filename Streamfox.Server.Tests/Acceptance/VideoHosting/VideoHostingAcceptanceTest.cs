namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Xunit;

    public class VideoHostingAcceptanceTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly ApplicationHost _applicationHost;

        public VideoHostingAcceptanceTest(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _applicationHost = new ApplicationHost(webApplicationFactory);
        }

        [Fact]
        public async Task UploadVideo_ThenDownload_ExpectExactCopyDownloaded()
        {
            byte[] videoBytes = await new File("Acceptance/VideoHosting/Video.mp4").ReadBytes();

            VideoId videoId = await _applicationHost.Post("/videos", videoBytes);
            byte[] downloadedVideoBytes = await _applicationHost.Get($"/videos/{videoId}");

            Assert.Equal(videoBytes, downloadedVideoBytes);
        }
    }
}