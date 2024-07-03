using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Social.Request
{
    public class PostRequestModel
    {
        [Required]
        public string? Subject { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? Video { get; set; }

        public string? Picture { get; set; }
    }
}
