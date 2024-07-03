using EduSpaceEngine.Model.Learn.Test;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn
{
    public class SubjectModel
    {
        [Key]
        public int SubjectId { get; set; }

        public string? SubjectName_ka { get; set; }
        public string? SubjectName_en { get; set; }

        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? LogoURL { get; set; }


        public int CourseId { get; set; }

        [JsonIgnore]
        public virtual CourseModel Course { get; set; }

        public virtual ICollection<LessonModel> Lessons { get; set; } = new List<LessonModel>();
    }
}
