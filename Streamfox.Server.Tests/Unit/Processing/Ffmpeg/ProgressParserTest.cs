namespace Streamfox.Server.Tests.Unit.Processing.Ffmpeg
{
    using Streamfox.Server.Processing.Ffmpeg;
    using Streamfox.Server.Types;

    using Xunit;

    public class ProgressParserTest
    {
        public static object[][] FrameLinesTestData => new[]
        {
            new object[] { "frame=   72 fps=0.0 q=31.0 size=       0kB", new ProgressReport(72) },
            new object[] { "frame=  109 fps=107 q=31.0 size=     768kB", new ProgressReport(109) },
            new object[] { "frame=  146 fps= 95 q=31.0 size=    1536kB", new ProgressReport(146) },
            new object[] { "frame=  178 fps= 87 q=31.0 size=    2560kB", new ProgressReport(178) },
            new object[] { "frame=  213 fps= 82 q=31.0 size=    3584kB", new ProgressReport(213) },
            new object[] { "frame=  252 fps= 81 q=31.0 size=    4608kB", new ProgressReport(252) },
            new object[] { "frame=  293 fps= 81 q=31.0 size=    5632kB", new ProgressReport(293) }
        };

        public static object[][] ValidInvalidTestData => new[]
        {
new object[] { "[libx264 @ 00000294b9eb0780] profile High, level 4.2, 4:2:0, 8-bit", false },
new object[] { "[libx264 @ 00000294b9eb0780] 264 - core 161 - H.264/MPEG-4 AVC codec - Copyleft 2003-2020 - http://www.videolan.org/x264.html - options: cabac=1 ref=3 deblock=1:0:0 analyse=0x3:0x113 me=hex subme=7 psy=1 psy_rd=1.00:0.00 mixed_ref=1 me_range=16 chroma_me=1 trellis=1 8x8dct=1 cqm=0 deadzone=21,11 fast_pskip=1 chroma_qp_offset=-2 threads=24 lookahead_threads=4 sliced_threads=0 nr=0 decimate=1 interlaced=0 bluray_compat=0 constrained_intra=0 bframes=3 b_pyramid=2 b_adapt=1 b_bias=0 direct=1 weightb=1 open_gop=0 weightp=2 keyint=250 keyint_min=25 scenecut=40 intra_refresh=0 rc_lookahead=40 rc=crf mbtree=1 crf=23.0 qcomp=0.60 qpmin=0 qpmax=69 qpstep=4 ip_ratio=1.40 aq=1:1.00", false },
new object[] { "Output #0, mp4, to 'csgo-h264.mp4':", false },
new object[] { "  Metadata:", false },
new object[] { "frame=   72 fps=0.0 q=31.0 size=       0kB time=00:00:01.39 bitrate=   0.3kbits/s speed=2.78x", true },
new object[] { "frame=  109 fps=107 q=31.0 size=     768kB time=00:00:02.04 bitrate=3079.2kbits/s speed=2.01x", true },
new object[] { "frame=  146 fps= 95 q=31.0 size=    1536kB time=00:00:02.64 bitrate=4753.7kbits/s speed=1.71x", true },
new object[] { "frame=  178 fps= 87 q=31.0 size=    2560kB time=00:00:03.18 bitrate=6592.6kbits/s speed=1.55x", true },
new object[] { "frame=  213 fps= 82 q=31.0 size=    3584kB time=00:00:03.76 bitrate=7805.3kbits/s speed=1.44x", true },
new object[] { "frame=  708 fps= 73 q=-1.0 Lsize=   16697kB time=00:00:11.81 bitrate=11573.1kbits/s speed=1.22x", true },
new object[] { "[libx264 @ 00000294b9eb0780] ref P L0: 64.3% 22.0%  9.3%  4.3%", false },
new object[] { "[libx264 @ 00000294b9eb0780] ref B L0: 91.8%  6.6%  1.6%", false },
new object[] { "[libx264 @ 00000294b9eb0780] ref B L1: 97.7%  2.3%", false },
new object[] { "[libx264 @ 00000294b9eb0780] kb/s:11459.93", false },
new object[] { "[aac @ 00000294b9ea57c0] Qavg: 7662.478", false }
        };

        [Theory]
        [MemberData(nameof(FrameLinesTestData))]
        public void ParseProgress(string input, ProgressReport expectedReport)
        {
            Optional<ProgressReport> actualReport = ProgressParser.ParseProgress(input);

            Assert.True(actualReport.HasValue);
            Assert.Equal(expectedReport.Frame, actualReport.Value.Frame);
        }

        [Theory]
        [MemberData(nameof(ValidInvalidTestData))]
        public void CatchesInvalidData(string input, bool isValid)
        {
            Optional<ProgressReport> actualReport = ProgressParser.ParseProgress(input);

            Assert.Equal(isValid, actualReport.HasValue);
        }
    }
}