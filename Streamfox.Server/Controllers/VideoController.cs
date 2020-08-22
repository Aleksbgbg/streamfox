namespace Streamfox.Server.Controllers
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Streamfox.Server.Controllers.Responses;
    using Streamfox.Server.Controllers.Results;
    using Streamfox.Server.Processing;
    using Streamfox.Server.Types;
    using Streamfox.Server.VideoManagement;

    [ApiController]
    [Route("api/videos")]
    public class VideoController : ControllerBase
    {
        private readonly IVideoClerk _videoClerk;

        public VideoController(IVideoClerk videoClerk)
        {
            _videoClerk = videoClerk;
        }

        [HttpPost]
        public async Task<IActionResult> PostVideo([FromBody] Stream stream)
        {
            Optional<VideoId> videoId = await _videoClerk.StoreVideo(stream);

            if (videoId.HasValue)
            {
                return Ok(new VideoIdResponse(videoId.Value));
            }

            return BadRequest();
        }

        [HttpGet("{videoId}")]
        public async Task<IActionResult> GetVideo(VideoId videoId)
        {
            Optional<StoredVideo> optionalStoredVideo = await _videoClerk.RetrieveVideo(videoId);

            if (optionalStoredVideo.HasValue)
            {
                StoredVideo storedVideo = optionalStoredVideo.Value;
                return Stream(storedVideo.VideoStream,
                        storedVideo.VideoMetadata.VideoFormat == VideoFormat.Webm ? "video/webm" : "video/mp4");
            }

            return NotFound();
        }

        [HttpGet]
        public OkObjectResult GetVideos()
        {
            return Ok(new VideoListResponse(_videoClerk.ListVideos()));
        }

        [HttpGet("{videoId}/thumbnail")]
        public IActionResult GetThumbnail(VideoId videoId)
        {
            Optional<Stream> stream = _videoClerk.RetrieveThumbnail(videoId);

            if (stream.HasValue)
            {
                return Stream(stream.Value, "image/jpeg");
            }

            return NotFound();
        }

        private static StreamResult Stream(Stream stream, string contentType)
        {
            return new StreamResult(stream, contentType);
        }
    }
}