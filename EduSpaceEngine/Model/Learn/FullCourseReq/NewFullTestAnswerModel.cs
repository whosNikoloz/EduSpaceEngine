using EduSpaceEngine.Model.Learn.Test;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.FullCourseReq
{
    public class NewFullTestAnswerModel
    {
        [Required]
        public string Option { get; set; }

        public bool IsCorrect { get; set; }
    }
}
