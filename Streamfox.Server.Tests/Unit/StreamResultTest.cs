﻿namespace Streamfox.Server.Tests.Unit
{
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;

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