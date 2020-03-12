namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    public class VideoHost
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public VideoHost(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public async Task<VideoId> Upload(byte[] videoBytes)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.PostAsync("/videos", new ByteArrayContent(videoBytes));

            string downloadUrl = await response.Content.ReadAsStringAsync();
            string videoId = downloadUrl.Split('/').Last();

            return new VideoId(long.Parse(videoId));
        }

        public async Task<ByteStream> Download(VideoId videoId)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync($"/videos/{videoId.Value}");
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

            return new ByteStream(bytes);
        }
    }
}