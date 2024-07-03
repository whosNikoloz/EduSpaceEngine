namespace EduSpaceEngine.Model.User
{
    public class CheckOauth2ExistsReqeust
    {
        public string? OAuthProvider { get; set; } // Store the OAuth provider (e.g., "Google")
        public string? OAuthProviderId { get; set; } // Store the unique identifier provided by the OAuth provider
    }
}
