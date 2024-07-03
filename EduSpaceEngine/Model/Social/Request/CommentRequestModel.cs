using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Social.Request
{
    public class CommentRequestModel
    {
        [Required]
        public string? Content { get; set; }

        public string? Picture { get; set; }

        public string? Video { get; set; }
    }
}
