namespace Streamfox.Server.Controllers.Formatters
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Net.Http.Headers;

    using Streamfox.Server.Utilities;

    public class RawByteStreamBodyFormatter : InputFormatter
    {
        public RawByteStreamBodyFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
                InputFormatterContext context)
        {
            return await InputFormatterResult.SuccessAsync(
                    await Streams.CloneToMemory(context.HttpContext.Request.Body));
        }
    }
}