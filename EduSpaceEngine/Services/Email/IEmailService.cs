namespace EduSpaceEngine.Services.Email
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string user, string confirmationLink);
        Task SendEmailAsync(string email, string user, string confirmationLink);
        Task SendWarningEmailAsync(string email, string user);
        Task SendChangeEmailCodeAsync(string email, string user, int randomNumber);
    }
}
