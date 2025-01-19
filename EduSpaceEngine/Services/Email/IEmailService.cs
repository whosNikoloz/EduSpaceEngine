namespace EduSpaceEngine.Services.Email
{
    public interface IEmailService
    {
        Task<string> SendVerificationEmailAsync(string email, string user, string confirmationLink);
        Task<string> SendEmailAsync(string email, string user, string confirmationLink);
        Task<string> SendWarningEmailAsync(string email, string user);
        Task<string> SendChangeEmailCodeAsync(string email, string user, int randomNumber);
    }
}
