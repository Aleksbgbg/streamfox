namespace Streamfox.Server.Tests.Unit.Controllers.Formatters
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;

    using Streamfox.Server.Controllers.Formatters;

    using Xunit;

    public class RawByteStreamBodyFormatterTest
    {
        private readonly RawByteStreamBodyFormatter _rawByteStreamBodyFormatter;

        public RawByteStreamBodyFormatterTest()
        {
            _rawByteStreamBodyFormatter = new RawByteStreamBodyFormatter();
        }

        [Fact]
        public void CanReadOctetStream()
        {
            Assert.True(
                    _rawByteStreamBodyFormatter.CanRead(
                            ContextForMediaType("application/octet-stream")));
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3 })]
        [InlineData(new byte[] { 4, 5, 6 })]
        public async Task ReadByteArray(byte[] bytes)
        {
            InputFormatterResult response = await _rawByteStreamBodyFormatter.ReadRequestBodyAsync(
                    ContextForMediaTypeWithContent("application/octet-stream", bytes));

            Stream stream = response.Model as Stream;
            Assert.NotNull(stream);
            Assert.Equal(bytes, TestUtils.ReadStreamBytes(stream));
        }

        private static InputFormatterContext ContextForMediaType(string mediaType)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = mediaType;

            return TestUtils.InputFormatterContextFor(httpContext);
        }

        private static InputFormatterContext ContextForMediaTypeWithContent(
                string mediaType, byte[] content)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = mediaType;
            httpContext.Request.Body = new MemoryStream(content);

            return TestUtils.InputFormatterContextFor(httpContext);
        }
    }
}