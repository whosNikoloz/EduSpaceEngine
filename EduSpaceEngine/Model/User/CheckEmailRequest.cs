using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.User
{
    public class CheckEmailRequest
    {
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
