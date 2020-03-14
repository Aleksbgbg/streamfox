namespace Streamfox.Server.Tests.Unit.Controllers.Formatters
{
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;

    using Streamfox.Server.Controllers.Formatters;
    using Streamfox.Server.VideoManagement;

    using Xunit;

    public class VideoIdParameterFormatterTest
    {
        private readonly VideoIdParameterFormatter _videoIdParameterFormatter;

        public VideoIdParameterFormatterTest()
        {
            _videoIdParameterFormatter = new VideoIdParameterFormatter();
        }

        [Fact]
        public void AcceptsVideoIdType()
        {
            Assert.True(_videoIdParameterFormatter.CanRead(SetupFormatterContext()));
        }

        [Fact]
        public async Task ParsesValidVideoId()
        {
            const string videoIdString = "123";

            InputFormatterResult result = await _videoIdParameterFormatter.ReadAsync(SetupFormatterContext(videoIdString));

            Assert.IsType<VideoId>(result.Model);
            VideoId videoId = (VideoId)result.Model;
            Assert.Equal(123, videoId.Value);
        }

        [Fact]
        public async Task ReturnsFailedResultForInvalidVideoId()
        {
            const string videoIdString = "9999999999999999999";

            InputFormatterResult result = await _videoIdParameterFormatter.ReadAsync(SetupFormatterContext(videoIdString));

            Assert.True(result.HasError);
        }

        private static InputFormatterContext SetupFormatterContext(string value = "")
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues = new RouteValueDictionary
            {
                { "videoId", value }
            };

            ParameterInfo videoIdParam = typeof(VideoIdParameterFormatterTest)
                                         .GetMethod(nameof(DummyMethod), BindingFlags.NonPublic | BindingFlags.Static)
                                         .GetParameters()[0];
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForParameter(videoIdParam, typeof(VideoId));

            return new InputFormatterContext(httpContext,
                                             modelName: string.Empty,
                                             modelState: new ModelStateDictionary(),
                                             modelMetadata,
                                             readerFactory: (_, __) => TextReader.Null);
        }

        private static void DummyMethod(VideoId videoId)
        {
        }
    }
}