using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Learn
{
    public class TestAnswerDto
    {
        [Required]
        public string Option_en { get; set; }

        [Required]
        public string Option_ka { get; set; }

        public bool IsCorrect { get; set; }
    }
}
