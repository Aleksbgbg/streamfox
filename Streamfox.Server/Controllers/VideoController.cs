namespace Streamfox.Server.Controllers
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.Controllers.Results;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    [ApiController]
    [Route("videos")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoClerk _videoClerk;

        public VideoController(IVideoClerk videoClerk)
        {
            _videoClerk = videoClerk;
        }

        [HttpGet("{videoId}")]
        public IActionResult GetVideo(VideoId videoId)
        {
            Optional<Stream> stream = _videoClerk.RetrieveVideo(videoId);

            if (stream.HasValue)
            {
                return Stream(stream.Value);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<CreatedResult> PostVideo([FromBody] Stream stream)
        {
            VideoId videoId = await _videoClerk.StoreVideo(stream);
            return Created($"/videos/{videoId}", new VideoMetadata(videoId));
        }

        private static StreamResult Stream(Stream stream)
        {
            return new StreamResult(stream);
        }
    }
}