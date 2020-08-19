namespace Streamfox.Server.Tests
{
    using System;
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

        public static string ReadStream(Stream stream)
        {
            stream.Position = 0;
            return new StreamReader(stream).ReadToEnd();
        }

        public static byte[] ReadStreamBytes(Stream stream)
        {
            return Encoding.ASCII.GetBytes(ReadStream(stream));
        }

        public static InputFormatterContext InputFormatterContextFor(HttpContext httpContext)
        {
            ModelMetadata modelMetadata =
                    new EmptyModelMetadataProvider().GetMetadataForType(typeof(void));

            return InputFormatterContextFor(httpContext, modelMetadata);
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


        public static byte[] CutBuffer(byte[] buffer, int size)
        {
            byte[] newBuffer = new byte[size];
            Array.Copy(buffer, newBuffer, newBuffer.Length);
            return newBuffer;
        }

        public static byte[] CutBuffer(byte[] buffer, int start, int end)
        {
            byte[] newBuffer = new byte[end - start];
            Array.Copy(buffer, start, newBuffer, 0, newBuffer.Length);
            return newBuffer;
        }
    }
}