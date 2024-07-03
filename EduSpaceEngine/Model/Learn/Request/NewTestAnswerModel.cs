using EduSpaceEngine.Model.Learn.Test;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.Request
{
    public class NewTestAnswerModel
    {

        [Required]
        public string Option { get; set; }

        public bool IsCorrect { get; set; }
    }
}
