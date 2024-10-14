using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn
{
    public class CourseModel
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        public string? CourseName_ka { get; set; }
        [Required]
        public string? CourseName_en { get; set; }

        public string? FormattedCourseName { get; set; }
        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? CourseLogo { get; set; }

        public int LevelId { get; set; } // Foreign key property

        [JsonIgnore]

        public virtual LevelModel Level { get; set; }

        public virtual ICollection<SubjectModel> Subjects { get; set; }

        [JsonIgnore]
        public virtual ICollection<ProgressModel> Progresses { get; set; } = new List<ProgressModel>();

        public virtual ICollection<CourseEnrollmentModel> Enrollments { get; set; } = new List<CourseEnrollmentModel>();

    }
}
