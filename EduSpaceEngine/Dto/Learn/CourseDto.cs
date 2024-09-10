using EduSpaceEngine.Model.Learn;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Learn
{
    public class CourseDto
    {
        [Required]
        public string? CourseName_ka { get; set; }
        [Required]
        public string? CourseName_en { get; set; }

        public string? FormattedCourseName { get; set; }
        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? CourseLogo { get; set; }

    }
}
