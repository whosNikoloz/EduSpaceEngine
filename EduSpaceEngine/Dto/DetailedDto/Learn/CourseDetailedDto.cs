using EduSpaceEngine.Model.Learn;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Learn.DetailedDto.Learn
{
    public class CourseDetailedDto
    {
        [Required]
        public string? CourseName_ka { get; set; }
        [Required]
        public string? CourseName_en { get; set; }

        public string? FormattedCourseName { get; set; }
        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? CourseLogo { get; set; }
        public virtual ICollection<SubjectModel>? Subjects { get; set; } = new List<SubjectModel>();

    }
}
