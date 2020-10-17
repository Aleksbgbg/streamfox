namespace Streamfox.Server.Tests.Unit.Processing.Ffmpeg
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class VideoFramesTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { ReadVideoMetadataFile("vp9"), CountFrames(1568.46d) };
            yield return new object[] { ReadVideoMetadataFile("h264"), CountFrames(1567d) };
            yield return new object[] { ReadVideoMetadataFile("h264-notmp4"), CountFrames(3600.00999d) };
            yield return new object[] { ReadVideoMetadataFile("h265"), CountFrames(1567d) };
            yield return new object[] { ReadVideoMetadataFile("audio"), CountFrames(0d) };
            yield return new object[] { ReadVideoMetadataFile("jpeg"), CountFrames(0d) };
            yield return new object[] { ReadVideoMetadataFile("text"), CountFrames(0d) };
            yield return new object[] { ReadVideoMetadataFile("not-parseable"), CountFrames(0d) };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static string ReadVideoMetadataFile(string name)
        {
            return TestFileReader.ReadString("VideoMetadataExamples", $"{name}.json");
        }

        private static int CountFrames(double frameCalculation)
        {
            return (int)Math.Round(frameCalculation);
        }
    }
}