using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Social
{
    public class CommentDto
    {
        [Required]
        public string? Content { get; set; }

        public string? Picture { get; set; }

        public string? Video { get; set; }
    }
}
