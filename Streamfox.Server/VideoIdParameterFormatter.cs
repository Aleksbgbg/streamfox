namespace Streamfox.Server
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Formatters;

    public class VideoIdParameterFormatter : IInputFormatter
    {
        public bool CanRead(InputFormatterContext context)
        {
            return context.ModelType == typeof(VideoId);
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            string parameterName = context.Metadata.ParameterName;
            string parameterValue = (string)context.HttpContext.Request.RouteValues[parameterName];

            if (long.TryParse(parameterValue, out long value))
            {
                VideoId videoId = new VideoId(value);
                return InputFormatterResult.SuccessAsync(videoId);
            }

            return InputFormatterResult.FailureAsync();
        }
    }
}