using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YouTubeAPI.Model
{
    public partial class Comments
    {
        public int CommentsId { get; set; }
        public int? VideoKey { get; set; }
        [Required]
        [Column("Comments")]
        [StringLength(255)]
        public string Comments1 { get; set; }
        [Required]
        [StringLength(255)]
        public string UserId { get; set; }

        [ForeignKey("VideoKey")]
        [InverseProperty("Comments")]
        public virtual Video VideoKeyNavigation { get; set; }
    }
}
