namespace Streamfox.Server.Tests
{
    using System.IO;
    using System.Text;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    using Moq;

    public static class TestUtils
    {
        public static Stream MockStream()
        {
            return new Mock<Stream>().Object;
        }

        public static byte[] ReadStreamBytes(Stream stream)
        {
            stream.Position = 0;
            return Encoding.ASCII.GetBytes(new StreamReader(stream).ReadToEnd());
        }

        public static InputFormatterContext InputFormatterContextFor(HttpContext httpContext)
        {
            ModelMetadata modelMetadata =
                    new EmptyModelMetadataProvider().GetMetadataForType(typeof(void));

            return new InputFormatterContext(httpContext,
                                             modelName: string.Empty,
                                             modelState: new ModelStateDictionary(),
                                             modelMetadata,
                                             readerFactory: (_, __) => TextReader.Null);
        }

        public static InputFormatterContext InputFormatterContextFor(
                HttpContext httpContext, ModelMetadata modelMetadata)
        {
            return new InputFormatterContext(httpContext,
                                             modelName: string.Empty,
                                             modelState: new ModelStateDictionary(),
                                             modelMetadata,
                                             readerFactory: (_, __) => TextReader.Null);
        }
    }
}