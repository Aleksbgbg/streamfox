namespace Streamfox.Server.Persistence.Database
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Tag
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Value { get; set; }

        public ICollection<VideoTag> VideoTags { get; set; }
    }
}