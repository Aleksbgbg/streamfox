namespace Streamfox.Server.Tests.Unit.Processing
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using Streamfox.Server.Processing;

    public class VideoMetadataTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { ReadVideoMetadataFile("vp9"), new VideoMetadata(VideoCodec.Vp9, VideoFormat.Webm) };
            yield return new object[] { ReadVideoMetadataFile("h264"), new VideoMetadata(VideoCodec.H264, VideoFormat.Mp4) };
            yield return new object[] { ReadVideoMetadataFile("h264-notmp4"), new VideoMetadata(VideoCodec.H264, VideoFormat.Other) };
            yield return new object[] { ReadVideoMetadataFile("h265"), new VideoMetadata(VideoCodec.Other, VideoFormat.Mp4) };
            yield return new object[] { ReadVideoMetadataFile("audio"), new VideoMetadata(VideoCodec.Invalid, VideoFormat.Mp4) };
            yield return new object[] { ReadVideoMetadataFile("jpeg"), new VideoMetadata(VideoCodec.Invalid, VideoFormat.Other) };
            yield return new object[] { ReadVideoMetadataFile("text"), new VideoMetadata(VideoCodec.Invalid, VideoFormat.Other) };
            yield return new object[] { ReadVideoMetadataFile("not-parseable"), new VideoMetadata(VideoCodec.Invalid, VideoFormat.Other) };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static string ReadVideoMetadataFile(string name)
        {
            return File.ReadAllText($"Unit/Processing/MetadataTestData/{name}.json");
        }
    }
}