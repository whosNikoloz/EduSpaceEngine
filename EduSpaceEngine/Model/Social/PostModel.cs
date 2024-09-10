using EduSpaceEngine.Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Social
{
    public class PostModel
    {
        [Key]
        public int PostId { get; set; }

        public string? Subject { get; set; }

        public string? Content { get; set; }

        public string? Video { get; set; }

        public string? Picture { get; set; }

        public DateTime CreateDate { get; set; }

        public int UserId { get; set; }

        [JsonIgnore] // Ignore this property during serialization
        public virtual UserModel? User { get; set; } // Navigation property to UserModel

        public virtual ICollection<CommentModel> Comments { get; set; }
    }
}
