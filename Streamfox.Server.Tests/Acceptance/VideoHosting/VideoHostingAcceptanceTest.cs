namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoHostingAcceptanceTest : IClassFixture<WebApplicationFactory<Startup>>, IDisposable
    {
        private readonly ApplicationHost _applicationHost;

        public VideoHostingAcceptanceTest(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _applicationHost = new ApplicationHost(webApplicationFactory);
        }

        public void Dispose()
        {
            Directory.Delete("Videos", recursive: true);
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