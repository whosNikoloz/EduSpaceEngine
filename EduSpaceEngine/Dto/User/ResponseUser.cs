using EduSpaceEngine.Model;

namespace EduSpaceEngine.Dto.User
{
    public class ResponseUser
    {
        public UserDto User { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
    }
}
