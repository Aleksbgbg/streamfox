namespace Streamfox.Server.Processing
{
    using Newtonsoft.Json;

    public class FfmpegVideoMetadata
    {
        [JsonConstructor]
        public FfmpegVideoMetadata(AudioVideoStream[] streams, Format format)
        {
            Streams = streams;
            Format = format;
        }

        public AudioVideoStream[] Streams { get; }

        public Format Format { get; }
    }

    public class AudioVideoStream
    {
        [JsonProperty("codec_name")]
        public string CodecName { get; set; }

        [JsonProperty("codec_type")]
        public string CodecType { get; set; }

        [JsonProperty("r_frame_rate")]
        public string FrameRate { get; set; }

        [JsonProperty("nb_frames")]
        public string Frames { get; set; }
    }

    public class Format
    {
        [JsonProperty("format_name")]
        public string FormatName { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }
    }
}