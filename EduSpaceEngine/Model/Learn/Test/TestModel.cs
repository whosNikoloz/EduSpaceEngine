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
        public string? Instruction { get; set; }

        [Required]
        public string? Question { get; set; }

        public string? Code { get; set; }

        public string? Hint { get; set; }

        public int LearnId { get; set; }

        [JsonIgnore]
        public virtual LearnModel? Learn { get; set; } 


        public virtual ICollection<TestAnswerModel>? Answers { get; set; }

    }
}
