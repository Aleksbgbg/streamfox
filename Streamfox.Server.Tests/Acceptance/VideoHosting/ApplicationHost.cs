namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    public class ApplicationHost
    {
        private readonly WebApplicationFactory<Startup> _webApplicationFactory;

        public ApplicationHost(WebApplicationFactory<Startup> webApplicationFactory)
        {
            _webApplicationFactory = webApplicationFactory;
        }

        public async Task<byte[]> Get(string endpoint)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();

            return bytes;
        }

        public async Task<VideoId> Post(string endpoint, byte[] content)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            ByteArrayContent byteArrayContent = new ByteArrayContent(content);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = byteArrayContent
            };
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            HttpResponseMessage response = await httpClient.SendAsync(request);

            string downloadUrl = response.Headers.Location.OriginalString;
            string videoId = downloadUrl.Split('/').Last();

            return new VideoId(long.Parse(videoId));
        }
    }
}