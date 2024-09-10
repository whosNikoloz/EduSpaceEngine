using EduSpaceEngine.Model.Learn.Test;
using EduSpaceEngine.Model.Learn;

namespace EduSpaceEngine.Dto.Learn.DetailedDto.Learn
{
    public class LessonDetailedDto
    {
        public string? LessonName_ka { get; set; }
        public string? LessonName_en { get; set; }
        public virtual ICollection<LearnModel>? LearnMaterial { get; set; } = new List<LearnModel>();

    }
}
