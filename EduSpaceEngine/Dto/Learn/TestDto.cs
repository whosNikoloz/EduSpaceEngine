using EduSpaceEngine.Model.Learn.Test;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Learn
{
    public class TestDto
    {
        [Required]
        public string Instruction_en { get; set; }
        [Required]
        public string Instruction_ka { get; set; }

        [Required]
        public string Question_en { get; set; }

        [Required]
        public string Question_ka { get; set; }

        public string Code { get; set; }

        public string Hint_en { get; set; }
        public string Hint_ka { get; set; }


        public virtual ICollection<TestAnswerModel>? Answers { get; set; }
    }
}
