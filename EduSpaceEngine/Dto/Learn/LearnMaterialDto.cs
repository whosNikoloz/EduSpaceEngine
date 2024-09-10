using EduSpaceEngine.Model.Learn.Test;
using EduSpaceEngine.Model.Learn;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Dto.Learn
{
    public class LearnMaterialDto
    {

        [Required]
        public string? LearnName_en { get; set; }

        [Required]
        public string? LearnName_ka { get; set; }

        [Required]
        public string? Content_en { get; set; }

        [Required]
        public string? Content_ka { get; set; }

        public string? Code { get; set; }

    }
}
