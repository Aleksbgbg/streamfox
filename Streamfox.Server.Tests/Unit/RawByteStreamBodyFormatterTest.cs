namespace Streamfox.Server.Tests.Unit
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    using Xunit;

    public class RawByteStreamBodyFormatterTest
    {
        private readonly RawByteStreamBodyFormatter _rawByteStreamBodyFormatter;

        public RawByteStreamBodyFormatterTest()
        {
            _rawByteStreamBodyFormatter = new RawByteStreamBodyFormatter();
        }

        [Fact]
        public void SupportsOctetStreamContentType()
        {
            Assert.Contains("application/octet-stream", _rawByteStreamBodyFormatter.SupportedMediaTypes);
        }

        [Fact]
        public void CanReadOctetStream()
        {
            Assert.True(_rawByteStreamBodyFormatter.CanRead(ContextForMediaType("application/octet-stream")));
        }

        [Fact]
        public async Task ReadByteArray123()
        {
            byte[] bytes = { 1, 2, 3 };

            InputFormatterResult response =
                    await _rawByteStreamBodyFormatter
                            .ReadRequestBodyAsync(ContextForMediaTypeWithContent("application/octet-stream", bytes));

            Stream stream = response.Model as Stream;
            Assert.NotNull(stream);
            Assert.Equal(bytes, TestUtil.ReadStreamBytes(stream));
        }

        [Fact]
        public async Task ReadByteArray456()
        {
            byte[] bytes = { 4, 5, 6 };

            InputFormatterResult response =
                    await _rawByteStreamBodyFormatter
                            .ReadRequestBodyAsync(ContextForMediaTypeWithContent("application/octet-stream", bytes));

            Stream stream = response.Model as Stream;
            Assert.NotNull(stream);
            Assert.Equal(bytes, TestUtil.ReadStreamBytes(stream));
        }

        private static InputFormatterContext ContextForMediaType(string mediaType)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = mediaType;

            return InputFormatterContextFor(httpContext);
        }

        private static InputFormatterContext ContextForMediaTypeWithContent(string mediaType, byte[] content)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.ContentType = mediaType;
            httpContext.Request.Body = new MemoryStream(content);

            return InputFormatterContextFor(httpContext);
        }

        private static InputFormatterContext InputFormatterContextFor(HttpContext httpContext)
        {
            ModelMetadata modelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(void));

            return new InputFormatterContext(httpContext,
                                             modelName: string.Empty,
                                             modelState: new ModelStateDictionary(),
                                             modelMetadata,
                                             readerFactory: (_, __) => TextReader.Null);
        }
    }
}