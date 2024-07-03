using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn
{
    public class LevelModel
    {
        [Key]
        public int LevelId { get; set; }

        public string? LevelName_ka { get; set; }
        public string? LevelName_en { get; set; }

        public string? LogoURL { get; set; }

        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public virtual ICollection<CourseModel> Courses { get; set; } = new List<CourseModel>();
    }
}
