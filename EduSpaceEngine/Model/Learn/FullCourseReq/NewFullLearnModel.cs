using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.FullCourseReq
{
    public class NewFullLearnModel
    {
        [Required]
        public string? LearnName { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? Code { get; set; }

        public ICollection<NewFullTestModel> newTestModels { get; set; }
    }
}
