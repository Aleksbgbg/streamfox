namespace Streamfox.Server
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    public class StreamResult : IActionResult
    {
        public StreamResult(Stream stream)
        {
            Stream = stream;
        }

        public Stream Stream { get; }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            await Stream.CopyToAsync(context.HttpContext.Response.Body);
        }
    }
}