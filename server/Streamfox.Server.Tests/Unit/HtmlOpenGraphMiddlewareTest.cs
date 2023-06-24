namespace Streamfox.Server.Tests.Unit
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    using Xunit;

    public class HtmlOpenGraphMiddlewareTest
    {
        [Theory]
        [InlineData("123")]
        [InlineData("456")]
        [InlineData("726946832285958144")]
        public async Task InjectsOpenGraphTagsForCorrectVideo(string videoId)
        {
            HtmlOpenGraphMiddleware htmlOpenGraphMiddleware = new HtmlOpenGraphMiddleware(Next);
            HttpContext context = new DefaultHttpContext();
            context.Request.Path =
                    new PathString($"/streamfox/watch/{videoId}");

            await htmlOpenGraphMiddleware.InvokeAsync(context);

            Assert.Equal(HtmlWithMetaTags(videoId), TestUtils.ReadStream(context.Response.Body));
        }

        private async Task Next(HttpContext context)
        {
            await using StreamWriter streamWriter = new StreamWriter(
                    context.Response.Body,
                    leaveOpen: true);
            await streamWriter.WriteAsync(OriginalHtml);
        }

        private string OriginalHtml => @"
<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width,initial-scale=1.0'>
    <link rel='icon' href='/favicon.ico'>
    <title>client</title>
  <link href='/js/0.js' rel='prefetch'><link href='/js/1.js' rel='prefetch'><link href='/js/app.js' rel='preload' as='script'><link href='/js/chunk-vendors.js' rel='preload' as='script'></head>
  <body>
    <noscript>
      <strong>We're sorry but client doesn't work properly without JavaScript enabled. Please enable it to continue.</strong>
    </noscript>
    <div id='app'></div>
    <!-- built files will be auto injected -->
  <script type='text/javascript' src='/js/chunk-vendors.js'></script><script type='text/javascript' src='/js/app.js'></script></body>
</html>
";

        private string HtmlWithMetaTags(string videoId) => $@"
<!DOCTYPE html>
<html lang='en'>
  <head>
<meta property='og:site_name' content='Streamfox'/>
<meta property='og:title' content='{videoId}'/>
<meta property='og:type' content='video.other'/>
<meta property='og:image' content='https://iamaleks.dev/streamfox/api/video/{videoId}/thumbnail'/>
<meta property='og:url' content='https://iamaleks.dev/streamfox/watch/{videoId}'/>
<meta property='og:video' content='https://iamaleks.dev/streamfox/api/video/{videoId}'/>
<meta property='og:video:url' content='https://iamaleks.dev/streamfox/api/video/{videoId}'/>
<meta property='og:video:type' content='video/mp4'/>
    <meta charset='utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width,initial-scale=1.0'>
    <link rel='icon' href='/favicon.ico'>
    <title>client</title>
  <link href='/js/0.js' rel='prefetch'><link href='/js/1.js' rel='prefetch'><link href='/js/app.js' rel='preload' as='script'><link href='/js/chunk-vendors.js' rel='preload' as='script'></head>
  <body>
    <noscript>
      <strong>We're sorry but client doesn't work properly without JavaScript enabled. Please enable it to continue.</strong>
    </noscript>
    <div id='app'></div>
    <!-- built files will be auto injected -->
  <script type='text/javascript' src='/js/chunk-vendors.js'></script><script type='text/javascript' src='/js/app.js'></script></body>
</html>
";
    }
}