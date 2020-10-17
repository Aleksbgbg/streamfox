namespace Streamfox.Server.Tests.Unit.Processing.Ffmpeg
{
    using Streamfox.Server.Processing.Ffmpeg;

    using Xunit;

    public class ProgressParserTest
    {
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
        public void ParseProgress(string input, ProgressReport expectedReport)
        {
            ProgressReport actualReport = ProgressParser.ParseProgress(input);

            Assert.Equal(expectedReport.Frame, actualReport.Frame);
        }
    }
}