using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.Request
{
    public class NewTestModel
    {
        [Required]
        public string? Instruction { get; set; }

        [Required]
        public string? Question { get; set; }

        public string? Code { get; set; }

        public string? Hint { get; set; }
    }
}
