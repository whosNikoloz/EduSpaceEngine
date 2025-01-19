using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Social;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model
{

    public enum UserRole
    {
        Admin,
        User,
        Guest,
    }
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [StringLength(50, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 50 characters.")]
        public string? UserName { get; set; }
        [StringLength(50)]
        public string? FirstName { get; set; }
        [StringLength(50)]
        public string? LastName { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? Picture { get; set; }

        public byte[] PasswordHash { get; set; } = new byte[32];

        public byte[] PasswordSalt { get; set; } = new byte[32];

        public string? HashedOTP { get; set; }
        public string? OTPSalt { get; set; }
        public DateTime? OTPExpirationTime { get; set; }

        public DateTime VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public UserRole Role { get; set; } = UserRole.Guest;

        // OAuth-specific properties
        public string? OAuthProvider { get; set; } // Store the OAuth provider (e.g., "Google")
        public string? OAuthProviderId { get; set; } // Store the unique identifier provided by the OAuth provider

        public string Plan { get; set; } = "Basic";


        //Learn
        public virtual ICollection<CourseEnrollmentModel> Enrollments { get; set; } = new List<CourseEnrollmentModel>(); //კურსები რომელსაც ერთროულად გადის მაგალიტათ Javas და C# კურსი აქვს ერთროულად დაწყებული


        //Posts

        public ICollection<NotificationModel>? Notifications { get; set; } 
        public virtual ICollection<PostModel>? Posts { get; set; }
        public virtual ICollection<CommentModel>? Comments { get; set; }


        //Progress

        public virtual ICollection<ProgressModel>? Progresses { get; set; }
        public DateTime LastActivity { get; set; }

    }
}
