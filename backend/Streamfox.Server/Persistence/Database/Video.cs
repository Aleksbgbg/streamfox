namespace Streamfox.Server.Persistence.Database
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using Streamfox.Server.Processing;

    public class Video
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }

        public int Views { get; set; }

        public VideoCodec Codec { get; set; }

        public VideoFormat Format { get; set; }

        public ICollection<VideoTag> VideoTags { get; set; }
    }
}