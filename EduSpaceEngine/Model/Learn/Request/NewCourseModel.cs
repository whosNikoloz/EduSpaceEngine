using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.Request
{
    public class NewCourseModel
    {
        [Required]
        public string? CourseName_ka { get; set; }
        public string? CourseName_en { get; set; }

        public string? FormattedCourseName { get; set; }

        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? CourseLogo { get; set; }

        public int LevelId { get; set; }
    }
}
