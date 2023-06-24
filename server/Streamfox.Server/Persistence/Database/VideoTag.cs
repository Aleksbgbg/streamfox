namespace Streamfox.Server.Persistence.Database
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class VideoTag
    {
        [ForeignKey(nameof(Video))]
        public long VideoId { get; set; }

        public Video Video { get; set; }

        [ForeignKey(nameof(Tag))]
        public long TagId { get; set; }

        public Tag Tag { get; set; }
    }
}