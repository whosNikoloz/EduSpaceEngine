using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Social
{
    public class PostDto
    {
        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? Video { get; set; }

        public string? Picture { get; set; }
    }
}
