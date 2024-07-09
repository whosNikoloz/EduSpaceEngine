using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.FullCourseReq
{
    public class NewFullTestModel
    {
        [Required]
        public string? Instruction { get; set; }

        [Required]
        public string? Question { get; set; }

        public string? Code { get; set; }

        public string? Hint { get; set; }

        public ICollection<NewFullTestAnswerModel> Answers { get; set; }
    }
}
