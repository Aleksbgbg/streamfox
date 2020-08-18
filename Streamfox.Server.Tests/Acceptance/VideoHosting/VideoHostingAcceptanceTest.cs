namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Streamfox.Server.Controllers.Responses;
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
            Assert.Empty(Directory.GetFiles("Intermediate"));
            Directory.Delete("Intermediate", recursive: true);
        }

        [Fact]
        public async Task UploadVideo_ThenDownload_ExpectExactCopyDownloaded()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");

            VideoId videoId = await _applicationHost.Post("/api/videos", videoBytes);
            byte[] downloadedVideoBytes = await _applicationHost.Get($"/api/videos/{videoId}");

            Assert.Equal(videoBytes, downloadedVideoBytes);
        }

        [Fact]
        public async Task UploadVideo_ListVideos_HasNewUploadedVideos()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");

            VideoId videoId0 = await _applicationHost.Post("/api/videos", videoBytes);
            VideoId videoId1 = await _applicationHost.Post("/api/videos", videoBytes);
            VideoList videoList = await _applicationHost.Get<VideoList>("/api/videos");

            Assert.Equal(VideoIdsToStrings(videoId0, videoId1), videoList.VideoIds);
        }

        [Fact]
        public async Task UploadVideo_DownloadThumbnail_ExpectFirstFrameSnapshot()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");
            byte[] thumbnailBytes = await ReadTestFile("VideoThumbnail.jpg");

            VideoId videoId = await _applicationHost.Post("/api/videos", videoBytes);
            byte[] downloadedThumbnailBytes =
                    await _applicationHost.Get($"/api/videos/{videoId}/thumbnail");

            Assert.Equal(thumbnailBytes, downloadedThumbnailBytes);
        }

        private static Task<byte[]> ReadTestFile(string name)
        {
            return new File($"Acceptance/VideoHosting/{name}").ReadBytes();
        }

        private string[] VideoIdsToStrings(params VideoId[] videoIds)
        {
            return videoIds.Select(id => id.Value.ToString()).ToArray();
        }
    }
}