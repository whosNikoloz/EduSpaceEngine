using EduSpaceEngine.Model.User;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn
{
    public class CourseEnrollmentModel
    {
        public int UserId { get; set; }
        [JsonIgnore]
        public UserModel? User { get; set; }

        public int CourseId { get; set; }
        [JsonIgnore]
        public CourseModel? Course { get; set; }
    }
}
