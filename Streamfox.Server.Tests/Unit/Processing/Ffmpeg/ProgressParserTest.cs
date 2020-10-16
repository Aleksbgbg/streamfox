namespace Streamfox.Server.Tests.Unit.Processing.Ffmpeg
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Streamfox.Server.Processing.Ffmpeg;

    using Xunit;
    using Xunit.Extensions;

    public class ProgressParserTest
    {
        /*
         * frame=   72 fps=0.0 q=31.0 size=       0kB time=00:00:01.39 bitrate=   0.3kbits/s speed=2.78x    
frame=  109 fps=107 q=31.0 size=     768kB time=00:00:02.04 bitrate=3079.2kbits/s speed=2.01x    
frame=  146 fps= 95 q=31.0 size=    1536kB time=00:00:02.64 bitrate=4753.7kbits/s speed=1.71x    
frame=  178 fps= 87 q=31.0 size=    2560kB time=00:00:03.18 bitrate=6592.6kbits/s speed=1.55x    
frame=  213 fps= 82 q=31.0 size=    3584kB time=00:00:03.76 bitrate=7805.3kbits/s speed=1.44x    
frame=  252 fps= 81 q=31.0 size=    4608kB time=00:00:04.38 bitrate=8601.7kbits/s speed=1.41x    
frame=  293 fps= 81 q=31.0 size=    5632kB time=00:00:05.10 bitrate=9031.8kbits/s speed=1.41x    
frame=  331 fps= 80 q=31.0 size=    6656kB time=00:00:05.73 bitrate=9507.1kbits/s speed=1.38x    
frame=  369 fps= 79 q=31.0 size=    7680kB time=00:00:06.36 bitrate=9888.8kbits/s speed=1.36x    
frame=  412 fps= 80 q=31.0 size=    8704kB time=00:00:07.05 bitrate=10101.3kbits/s speed=1.36x    
frame=  449 fps= 79 q=31.0 size=    9472kB time=00:00:07.68 bitrate=10095.9kbits/s speed=1.35x    
frame=  488 fps= 78 q=31.0 size=   10496kB time=00:00:08.35 bitrate=10286.1kbits/s speed=1.34x    
frame=  530 fps= 79 q=31.0 size=   11520kB time=00:00:09.05 bitrate=10421.2kbits/s speed=1.34x    
frame=  559 fps= 77 q=31.0 size=   12288kB time=00:00:09.54 bitrate=10548.0kbits/s speed=1.31x    
frame=  599 fps= 77 q=31.0 size=   13568kB time=00:00:10.19 bitrate=10903.9kbits/s speed=1.31x    
frame=  637 fps= 77 q=31.0 size=   14336kB time=00:00:10.82 bitrate=10853.6kbits/s speed= 1.3x    
frame=  685 fps= 78 q=31.0 size=   15104kB time=00:00:11.63 bitrate=10636.1kbits/s speed=1.32x    
frame=  708 fps= 73 q=-1.0 Lsize=   16697kB time=00:00:11.81 bitrate=11573.1kbits/s speed=1.22x 
         */

        public static object[][] TestData => new[]
        {
            new object[] { "frame=   72 fps=0.0 q=31.0 size=       0kB", new ProgressReport(72) },
            new object[] { "frame=  109 fps=107 q=31.0 size=     768kB", new ProgressReport(109) },
            new object[] { "frame=  146 fps= 95 q=31.0 size=    1536kB", new ProgressReport(146) },
            new object[] { "frame=  178 fps= 87 q=31.0 size=    2560kB", new ProgressReport(178) },
            new object[] { "frame=  213 fps= 82 q=31.0 size=    3584kB", new ProgressReport(213) },
            new object[] { "frame=  252 fps= 81 q=31.0 size=    4608kB", new ProgressReport(252) },
            new object[] { "frame=  293 fps= 81 q=31.0 size=    5632kB", new ProgressReport(293) }
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public void name(string input, ProgressReport expectedReport)
        {
            ProgressReport actualReport = ProgressParser.ParseProgress(input);

            Assert.Equal(expectedReport.Frame, actualReport.Frame);
        }
    }
}