
namespace EduSpaceEngine.Dto
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Picture { get; set; }
        public string? Role { get; set; }
        public string? OAuthProvider { get; set; } // Optional: include only if OAuth is relevant
        public string? OAuthProviderId { get; set; } // Optional: include only if OAuth is relevant
        public string? VerificationToken { get; set; }
        public DateTime LastActivity { get; set; }
        public string Plan { get; set; } = "Basic";
    }
}
