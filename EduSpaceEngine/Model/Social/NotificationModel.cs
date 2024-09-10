using EduSpaceEngine.Model.User;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Social
{
    public class NotificationModel
    {
        [Key]
        public int NotificationId { get; set; }
        public string? Message { get; set; }

        public int PostId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? CommentAuthorUsername { get; set; }

        public string? CommentAuthorPicture { get; set; }

        public int UserId { get; set; }

        [JsonIgnore]
        public UserModel? User { get; set; }
    }
}
