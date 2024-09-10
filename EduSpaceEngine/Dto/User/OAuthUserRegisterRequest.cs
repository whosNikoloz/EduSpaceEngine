namespace EduSpaceEngine.Dto.User
{
    public class OAuthUserRegisterRequest
    {
        public string? email { get; set; }
        public string? username { get; set; }

        public string? picture { get; set; }
        public string? oAuthProvider { get; set; }
        public string? oAuthProviderId { get; set; }
    }
}
