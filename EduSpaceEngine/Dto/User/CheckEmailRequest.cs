using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.User
{
    public class CheckEmailRequest
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
