using EduSpaceEngine.Model.Learn.Test;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn
{
    public class LessonModel
    {
        [Key]
        public int LessonId { get; set; }
        public string? LessonName_ka { get; set; }
        public string? LessonName_en { get; set; }
        public int SubjectId { get; set; }

        [JsonIgnore]
        public virtual SubjectModel? Subject { get; set; }

        public virtual ICollection<LearnModel> LearnMaterial { get; set; } = new List<LearnModel>();
    }
}
