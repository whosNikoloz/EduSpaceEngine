using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.User.LoginRequest
{
    public class UserLoginUserNameRequest
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
