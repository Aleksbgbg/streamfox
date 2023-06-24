namespace Streamfox.Server.Controllers
{
    using System.Text.Json.Serialization;
    using System.Text.RegularExpressions;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/oembed")]
    public class OembedController : ControllerBase
    {
        public class Oembed
        {
            public int Height { get; set; }

            public int Width { get; set; }

            public string Html { get; set; }

            [JsonPropertyName("provider_name")]
            public string ProviderName { get; set; }

            [JsonPropertyName("provider_url")]
            public string ProviderUrl { get; set; }

            [JsonPropertyName("thumbnail_url")]
            public string ThumbnailUrl { get; set; }

            public string Title { get; set; }

            public string Type { get; set; }

            public string Version { get; set; }
        }



        [HttpGet]
        public IActionResult GetOembed([FromQuery] string url)
        {
            //{
            //    "height": 720,
            //    "html": "<iframe class=\"streamable-embed\" src=\"https://streamable.com/o/z3h4uv\" frameborder=\"0\" scrolling=\"no\" width=\"1280\" height=\"720\" allowfullscreen></iframe>",
            //    "provider_name": "Streamable",
            //    "provider_url": "https://streamable.com",
            //    "thumbnail_url": "//thumbs-east.streamable.com/image/z3h4uv.jpg?height=300",
            //    "title": "180",
            //    "type": "video",
            //    "version": "1.0",
            //    "width": 1280
            //}

            Match regexMatch = Regex.Match(url, @"^https://iamaleks\.dev/streamfox/watch/(?<videoId>\d+)$");

            if (regexMatch.Success)
            {
                string videoId = regexMatch.Groups["videoId"].Value;

                return Ok(
                        new Oembed
                        {
                            Height = 720,
                            Width = 1280,
                            Html =
                                    $"<iframe class='streamfox-embed' src='https://iamaleks.dev/streamfox/watch/{videoId}' frameborder='0' width='1280' height='720' allowfullscreen></iframe>",
                            ProviderName = "Streamfox",
                            ProviderUrl = "https://iamaleks.dev/streamfox",
                            ThumbnailUrl = $"https://iamaleks.dev/streamfox/api/videos/{videoId}/thumbnail",
                            Title = videoId,
                            Type = "video",
                            Version = "1.0"
                        });
            }

            return BadRequest();
        }
    }
}