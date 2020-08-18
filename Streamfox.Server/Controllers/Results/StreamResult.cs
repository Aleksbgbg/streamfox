namespace Streamfox.Server.Controllers.Results
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    public class StreamResult : IActionResult
    {
        public StreamResult(Stream stream, string contentType)
        {
            Stream = stream;
            ContentType = contentType;
        }

        public Stream Stream { get; }

        public string ContentType { get; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;
            await Stream.CopyToAsync(context.HttpContext.Response.Body);
        }
    }
}