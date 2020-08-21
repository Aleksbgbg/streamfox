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

    public class VideoHostingAcceptanceTest : IClassFixture<WebApplicationFactory<Startup>>,
                                              IDisposable
    {
        private readonly ApplicationHost _applicationHost;

        public VideoHostingAcceptanceTest(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _applicationHost = new ApplicationHost(webApplicationFactory);
        }

        public void Dispose()
        {
            //Directory.Delete("Videos", recursive: true);
            //Directory.Delete("Thumbnails", recursive: true);
            //Assert.Empty(Directory.GetFiles("Intermediate"));
            //Directory.Delete("Intermediate", recursive: true);
        }

        [Fact]
        public async Task UploadVideo_ThenDownload_ExpectExactCopyDownloaded()
        {
            byte[] videoBytes = await ReadTestFile("Video.mp4");

            VideoId videoId = await _applicationHost.Post("/api/videos", videoBytes);
            BytesResponse response = await _applicationHost.GetBytesAndContentType($"/api/videos/{videoId}");

            Assert.Equal(videoBytes, response.Bytes);
            Assert.Equal("video/mp4", response.ContentType);
        }

        [Fact]
        public async Task UploadH265Video_ThenDownload_ExpectVp9CopyDownloaded()
        {
            byte[] h265VideoBytes = await ReadTestFile("Video-h265.mp4");
            byte[] vp9VideoBytes = await ReadTestFile("Video-vp9.webm");

            VideoId videoId = await _applicationHost.Post("/api/videos", h265VideoBytes);
            BytesResponse response = await _applicationHost.GetBytesAndContentType($"/api/videos/{videoId}");

            double matchingPercent =
                    ((double)CountMatchingBytes(vp9VideoBytes, response.Bytes) /
                     vp9VideoBytes.Length) *
                    100d;
            Assert.True(matchingPercent > 99.99d);
            Assert.Equal("video/webm", response.ContentType);
        }

        [Fact]
        public async Task UploadVideo_ListVideos_HasNewUploadedVideos()
        {
            Directory.Delete("Videos", recursive: true);
            Directory.Delete("Thumbnails", recursive: true);
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
                    await _applicationHost.GetBytes($"/api/videos/{videoId}/thumbnail");

            Assert.Equal(thumbnailBytes.Length, downloadedThumbnailBytes.Length);
            Assert.Equal(thumbnailBytes, downloadedThumbnailBytes);
        }

        [Fact]
        public async Task UploadVideo_ThenStream_ExpectAccurateRangeStreaming()
        {
            const string streamRangeFirst5Mib = "bytes=0-";
            const string streamRangeNext5Mib = "bytes=5242880-";
            const string streamRangeEndOfVideo = "bytes=48568606-";
            byte[] videoBytes = await ReadTestFile("Video.mp4");

            VideoId videoId = await _applicationHost.Post("/api/videos", videoBytes);
            PartialResponse initial5MibStream = await GetVideoRange(videoId, streamRangeFirst5Mib);
            PartialResponse next5MibStream = await GetVideoRange(videoId, streamRangeNext5Mib);
            PartialResponse last1MibStream = await GetVideoRange(videoId, streamRangeEndOfVideo);

            Assert.Equal(5242880, initial5MibStream.ContentLength);
            Assert.Equal(0, initial5MibStream.Start);
            Assert.Equal(5242879, initial5MibStream.End);
            Assert.Equal(videoBytes.Length, initial5MibStream.TotalSize);
            Assert.Equal(TestUtils.CutBuffer(videoBytes, 0, 5242880), initial5MibStream.Bytes);

            Assert.Equal(5242880, next5MibStream.ContentLength);
            Assert.Equal(5242880, next5MibStream.Start);
            Assert.Equal(10485759, next5MibStream.End);
            Assert.Equal(videoBytes.Length, next5MibStream.TotalSize);
            Assert.Equal(TestUtils.CutBuffer(videoBytes, 5242880, 10485760), next5MibStream.Bytes);

            Assert.Equal(1048576, last1MibStream.ContentLength);
            Assert.Equal(48568606, last1MibStream.Start);
            Assert.Equal(videoBytes.Length - 1, last1MibStream.End);
            Assert.Equal(videoBytes.Length, last1MibStream.TotalSize);
            Assert.Equal(
                    TestUtils.CutBuffer(videoBytes, 48568606, videoBytes.Length),
                    last1MibStream.Bytes);
        }

        private static Task<byte[]> ReadTestFile(string name)
        {
            return new File($"Acceptance/VideoHosting/{name}").ReadBytes();
        }

        private static string[] VideoIdsToStrings(params VideoId[] videoIds)
        {
            return videoIds.Select(id => id.Value.ToString()).ToArray();
        }

        private Task<PartialResponse> GetVideoRange(VideoId videoId, string range)
        {
            return _applicationHost.GetRange($"/api/videos/{videoId}", range);
        }

        private static int CountMatchingBytes(byte[] a, byte[] b)
        {
            int matches = 0;

            for (int i = 0; i < a.Length; i++)
            {
                byte expected = a[i];
                byte actual = b[i];

                if (expected == actual)
                {
                    matches++;
                }
            }

            return matches;
        }
    }
}