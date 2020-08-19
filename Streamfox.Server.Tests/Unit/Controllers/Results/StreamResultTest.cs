namespace Streamfox.Server.Tests.Unit.Controllers.Results
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;

    using Streamfox.Server.Controllers.Results;

    using Xunit;

    public class StreamResultTest
    {
        [Fact]
        public async Task WritesStreamContentsToResponse()
        {
            StreamResult result = new StreamResult(new MemoryStream(StringToBytes("abc")));
            ActionContext actionContext = PrepareActionContext();

            await result.ExecuteResultAsync(actionContext);

            Stream body = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(3, body.Length);
            Assert.Equal((byte)'a', (byte)body.ReadByte());
            Assert.Equal((byte)'b', (byte)body.ReadByte());
            Assert.Equal((byte)'c', (byte)body.ReadByte());
        }

        [Fact]
        public async Task WritesContentTypeToResponse()
        {
            StreamResult result = new StreamResult(
                    new MemoryStream(StringToBytes("abc")),
                    "video/lmao420");
            ActionContext actionContext = PrepareActionContext();

            await result.ExecuteResultAsync(actionContext);

            Assert.Equal("video/lmao420", actionContext.HttpContext.Response.ContentType);
        }

        [Fact]
        public async Task Writes5MibOnHalfOpenRangeStart()
        {
            byte[] buffer = RandomBufferOfSize(Mib(6));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", "bytes=0-");

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(CutBuffer(buffer, Mib(5)), ReadStream(responseBody));
        }

        [Fact]
        public async Task Writes5MibOnHalfOpenRangeMiddle()
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", $"bytes={Mib(3)}-");

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(CutBuffer(buffer, Mib(3), Mib(8)), ReadStream(responseBody));
        }

        [Fact]
        public async Task WritesAllDataOnHalfOpenRangeEnd()
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", $"bytes={Mib(7)}-");

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(CutBuffer(buffer, Mib(7), Mib(10)), ReadStream(responseBody));
        }

        [Fact]
        public async Task Writes5MibOnClosedRangeMiddle()
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", $"bytes={Mib(3)}-{Mib(6)}");

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(CutBuffer(buffer, Mib(3), Mib(6) + 1), ReadStream(responseBody));
        }

        [Fact]
        public async Task WritesAllDataOnClosedRangeOverEnd()
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", $"bytes={Mib(7)}-{Mib(15)}");

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(CutBuffer(buffer, Mib(7), Mib(10)), ReadStream(responseBody));
        }

        [Fact]
        public async Task WritesAppropriateResponseHeaders()
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", $"bytes={Mib(3)}-{Mib(5)}");

            await result.ExecuteResultAsync(actionContext);

            HttpResponse response = actionContext.HttpContext.Response;
            Assert.Equal(206, response.StatusCode);
            Assert.Equal(Mib(5) - Mib(3) + 1, response.Headers.ContentLength);
            Assert.Equal(
                    $"bytes {Mib(3)}-{Mib(5)}/{Mib(10)}",
                    response.Headers["Content-Range"].First());
        }

        [Theory]
        [InlineData("bytes=-1-")]
        [InlineData("bytes=asdfasdf")]
        [InlineData("bytes=asdfasdf-asdfasdf")]
        [InlineData("bytes=50-1")]
        [InlineData("bytes=-1-50")]
        [InlineData("bytes=0-999999999")]
        [InlineData("bytes=bytes=0-50")]
        [InlineData("nights=0-50")]
        [InlineData("bytes=0--50")]
        [InlineData("bytes+0-50")]
        [InlineData("=bytes=0-50")]
        public async Task WritesEntireResponseToInvalidHeaders(string header)
        {
            byte[] buffer = RandomBufferOfSize(Mib(10));
            StreamResult result = new StreamResult(new MemoryStream(buffer));
            ActionContext actionContext = PrepareActionContext();
            actionContext.HttpContext.Request.Headers.Add("Range", header);

            await result.ExecuteResultAsync(actionContext);

            Stream responseBody = PrepareResponseStreamForAssertions(actionContext);
            Assert.Equal(buffer, ReadStream(responseBody));
        }

        private static int Mib(int quantity)
        {
            return 1024 * 1024 * quantity;
        }

        private static byte[] CutBuffer(byte[] buffer, int size)
        {
            return TestUtils.CutBuffer(buffer, size);
        }

        private static byte[] CutBuffer(byte[] buffer, int start, int end)
        {
            return TestUtils.CutBuffer(buffer, start, end);
        }

        private static byte[] RandomBufferOfSize(int size)
        {
            byte[] buffer = new byte[size];

            Random random = new Random();
            random.NextBytes(buffer);

            return buffer;
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        private static byte[] StringToBytes(string value)
        {
            return Encoding.ASCII.GetBytes(value);
        }

        private static ActionContext PrepareActionContext()
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        private static Stream PrepareResponseStreamForAssertions(ActionContext actionContext)
        {
            Stream body = actionContext.HttpContext.Response.Body;
            body.Position = 0;
            return body;
        }
    }
}