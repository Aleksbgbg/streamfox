namespace Streamfox.Server
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Http;

    public class HtmlOpenGraphMiddleware
    {
        private readonly RequestDelegate _next;

        public HtmlOpenGraphMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Match match = MatchUrlAgainstWatchPage(context);

            if (match.Success)
            {
                Stream originalBody = InjectCustomBodyStreamAndReturnOriginalBody(context);

                await _next(context);

                context.Response.Body = await InjectMetaTagsAfterHead(
                        context.Response.Body,
                        FormatInjectableMetaTags(FindVideoId(match)));
                context.Response.ContentLength = context.Response.Body.Length;
                await ReturnBody(context.Response, originalBody);
            }
            else
            {
                await _next(context);
            }
        }

        private static Match MatchUrlAgainstWatchPage(HttpContext context)
        {
            return Regex.Match(context.Request.Path.Value, @"^.*/watch/(?<videoId>\d+)$");
        }

        private static string FindVideoId(Match regexMatch)
        {
            return regexMatch.Groups["videoId"].Value;
        }

        private static Stream InjectCustomBodyStreamAndReturnOriginalBody(HttpContext context)
        {
            return ReplaceBody(context.Response);
        }

        private static string FormatInjectableMetaTags(string videoId)
        {
            return $@"
<meta property='og:site_name' content='Streamfox'>
<meta property='og:title' content='{videoId}'>
<meta property='og:type' content='video.other'>
<meta property='og:image' content='https://iamaleks.dev/streamfox/api/videos/{videoId}/thumbnail'>
<meta property='og:image:secure_url' content='https://iamaleks.dev/streamfox/api/videos/{videoId}/thumbnail'>
<meta property='og:image:type' content='image/jpeg'>
<meta property='og:image:width' content='1280'>
<meta property='og:image:height' content='720'>
<meta property='og:url' content='https://iamaleks.dev/streamfox/watch/{videoId}'>
<meta property='og:video' content='https://iamaleks.dev/streamfox/api/videos/{videoId}'>
<meta property='og:video:url' content='https://iamaleks.dev/streamfox/api/videos/{videoId}'>
<meta property='og:video:secure_url' content='https://iamaleks.dev/streamfox/api/videos/{videoId}'>
<meta property='og:video:type' content='video/mp4'>
<meta property='og:video:width' content='1280'>
<meta property='og:video:height' content='720'>

<meta name='twitter:card' content='player'>                                                                                             
<meta name='twitter:site' content='@streamfox'>                                                                                        
<meta name='twitter:image' content='https://iamaleks.dev/streamfox/api/videos/{videoId}/thumbnail'>
<meta name='twitter:player:width' content='1280'>                                                                                       
<meta name='twitter:player:height' content='720'>                                                                                       
<meta name='twitter:player' content='https://iamaleks.dev/streamfox/watch/{videoId}'>

<link rel='alternate' title='oEmbed' type='application/json+oembed' href='https://iamaleks.dev/streamfox/api/oembed?url=https://iamaleks.dev/streamfox/watch/{videoId}'/>";
        }

        private static async Task<Stream> InjectMetaTagsAfterHead(Stream body, string metaTags)
        {
            SearchStream(body, "<title>Streamfox</title>");
            int metaTagsInjectPosition = (int)body.Position;
            body.Position = 0;

            MemoryStream injectedStream = new MemoryStream();

            await using (StreamWriter injectedStreamWriter =
                    new StreamWriter(injectedStream, leaveOpen: true))
            {
                using (StreamReader bodyReader = new StreamReader(body))
                {
                    int totalRead = 0;
                    char[] buffer = new char[4096];

                    while (totalRead < metaTagsInjectPosition)
                    {
                        int currentRead = await bodyReader.ReadBlockAsync(
                                buffer,
                                0,
                                Math.Min(buffer.Length, metaTagsInjectPosition - totalRead));
                        totalRead += currentRead;
                        await injectedStreamWriter.WriteAsync(buffer, 0, currentRead);
                    }

                    await injectedStreamWriter.WriteAsync(metaTags);

                    while (totalRead < body.Length)
                    {
                        int currentRead = await bodyReader.ReadBlockAsync(buffer, 0, buffer.Length);
                        totalRead += currentRead;
                        await injectedStreamWriter.WriteAsync(buffer, 0, currentRead);
                    }
                }
            }

            return injectedStream;
        }

        private static void SearchStream(Stream stream, string substring)
        {
            stream.Position = 0;

            while (stream.Position < stream.Length)
            {
                char currentCharacter = '\0';

                while (currentCharacter != substring[0])
                {
                    currentCharacter = ReadChar(stream);
                }

                bool isMatch = true;

                for (int subChar = 1; subChar < substring.Length; ++subChar)
                {
                    currentCharacter = ReadChar(stream);

                    if (currentCharacter != substring[subChar])
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return;
                }
            }
        }

        private static char ReadChar(Stream stream)
        {
            return (char)stream.ReadByte();
        }

        private static Stream ReplaceBody(HttpResponse response)
        {
            Stream originalBody = response.Body;
            response.Body = new MemoryStream();
            return originalBody;
        }

        private static async Task ReturnBody(HttpResponse response, Stream originBody)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            await response.Body.CopyToAsync(originBody);
            response.Body = originBody;
        }
    }
}