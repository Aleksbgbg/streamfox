namespace Streamfox.Server.Tests.Unit.Controllers.Formatters
{
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

            InputFormatterResult result =
                    await _videoIdParameterFormatter.ReadAsync(
                            SetupFormatterContext(videoIdString));

            Assert.IsType<VideoId>(result.Model);
            VideoId videoId = (VideoId)result.Model;
            Assert.Equal(123, videoId.Value);
        }

        [Fact]
        public async Task ReturnsFailedResultForInvalidVideoId()
        {
            string videoIdString = $"9{long.MaxValue}";

            InputFormatterResult result =
                    await _videoIdParameterFormatter.ReadAsync(
                            SetupFormatterContext(videoIdString));

            Assert.True(result.HasError);
        }

        private static InputFormatterContext SetupFormatterContext(string videoIdString = "")
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues = new RouteValueDictionary
            {
                { "videoId", videoIdString }
            };

            ParameterInfo videoIdParam = typeof(VideoIdParameterFormatterTest)
                                         .GetMethod(
                                                 nameof(DummyMethod),
                                                 BindingFlags.NonPublic | BindingFlags.Static)
                                         .GetParameters()[0];

            ModelMetadata modelMetadata =
                    new EmptyModelMetadataProvider().GetMetadataForParameter(
                            videoIdParam,
                            typeof(VideoId));

            return TestUtils.InputFormatterContextFor(httpContext, modelMetadata);
        }

        private static void DummyMethod(VideoId videoId)
        {
        }
    }
}