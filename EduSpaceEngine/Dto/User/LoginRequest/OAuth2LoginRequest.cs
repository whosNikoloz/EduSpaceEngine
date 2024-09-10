namespace EduSpaceEngine.Dto.User.LoginRequest
{
    public class OAuth2LoginRequest
    {
        public string? OAuthProvider { get; set; } // Store the OAuth provider (e.g., "Google")
        public string? OAuthProviderId { get; set; } // Store the unique identifier provided by the OAuth provider
    }
}
