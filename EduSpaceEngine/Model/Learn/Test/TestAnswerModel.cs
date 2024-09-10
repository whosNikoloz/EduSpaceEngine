using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EduSpaceEngine.Model.Learn.Test
{
    public class TestAnswerModel
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public string Option_en { get; set; }

        [Required]
        public string Option_ka { get; set; }

        public bool IsCorrect { get; set; }

        // Foreign Key
        public int TestId { get; set; }

        // Navigation Property
        [JsonIgnore]
        public virtual TestModel? Test { get; set; }
    }
}
