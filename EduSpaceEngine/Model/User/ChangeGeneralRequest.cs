namespace EduSpaceEngine.Model.User
{
    public class ChangeGeneralRequest
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
