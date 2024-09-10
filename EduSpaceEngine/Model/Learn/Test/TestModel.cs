using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn.Test
{
    public class TestModel
    {
        [Key]
        public int TestId { get; set; }

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

        public int LearnId { get; set; }

        [JsonIgnore]
        public virtual LearnModel? Learn { get; set; } 


        public virtual ICollection<TestAnswerModel>? Answers { get; set; }

    }
}
