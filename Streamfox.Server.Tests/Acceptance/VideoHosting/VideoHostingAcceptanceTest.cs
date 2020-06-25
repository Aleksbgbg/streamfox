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
            Directory.Delete("Thumbnails", recursive: true);
        }

        [Fact]
        public async Task UploadVideo_ThenDownload_ExpectExactCopyDownloaded()
        {
            byte[] videoBytes = await ReadTestFile("Video2.mp4");

            VideoId videoId = await _applicationHost.Post("/videos", videoBytes);
            byte[] downloadedVideoBytes = await _applicationHost.Get($"/videos/{videoId}");

            Assert.Equal(videoBytes, downloadedVideoBytes);
        }

        [Fact]
        public async Task UploadVideo_ListVideos_HasNewUploadedVideos()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");

            VideoId videoId0 = await _applicationHost.Post("/videos", videoBytes);
            VideoId videoId1 = await _applicationHost.Post("/videos", videoBytes);
            long[] videoIds = await _applicationHost.Get<long[]>("/videos");

            Assert.Equal(new[] { videoId0.Value, videoId1.Value }, videoIds);
        }

        [Fact]
        public async Task UploadVideo_DownloadThumbnail_ExpectFirstFrameSnapshot()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");
            byte[] thumbnailBytes = await ReadTestFile("VideoThumbnail.jpg");

            VideoId videoId = await _applicationHost.Post("/videos", videoBytes);
            byte[] downloadedThumbnailBytes =
                    await _applicationHost.Get($"/videos/{videoId}/thumbnail");

            Assert.Equal(thumbnailBytes, downloadedThumbnailBytes);
        }

        private static Task<byte[]> ReadTestFile(string name)
        {
            return new File($"Acceptance/VideoHosting/{name}").ReadBytes();
        }
    }
}