using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.Test
{
    public class VideoModel
    {
        [Key]
        public int VideoId { get; set; }

        [Required]
        public string? VideoUrl { get; set; }

        [Required]
        public string? VideName { get; set; }

        public string? Description { get; set; }

        [ForeignKey("LearnId")]
        public virtual LearnModel? Learn { get; set; }
    }
}
