using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Social
{
    public class CommentModel
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string? Content { get; set; }

        public string? Picture { get; set; }

        public string? Video { get; set; }

        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }


        [JsonIgnore] // Ignore this property during serialization
        public virtual PostModel? Post { get; set; } // Navigation property to Post

        public int UserId { get; set; }


        [JsonIgnore] // Ignore this property during serialization
        public virtual UserModel? User { get; set; }
    }
}
