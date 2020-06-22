namespace Streamfox.Server.Tests.Acceptance.VideoHosting
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Testing;

    using Newtonsoft.Json;

    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.VideoManagement;

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

        public async Task<T> Get<T>(string endpoint)
        {
            HttpClient httpClient = _webApplicationFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync(endpoint);
            string content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
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
            string responseString = await response.Content.ReadAsStringAsync();

            VideoMetadata videoMetadata = JsonConvert.DeserializeObject<VideoMetadata>(responseString);

            return new VideoId(long.Parse(videoMetadata.VideoId));
        }
    }
}