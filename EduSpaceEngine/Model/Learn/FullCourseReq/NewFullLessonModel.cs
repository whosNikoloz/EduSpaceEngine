using EduSpaceEngine.Model.Learn.Test;
using System.ComponentModel.DataAnnotations;

namespace EduSpaceEngine.Model.Learn.FullCourseReq
{
    public class NewFullLessonModel
    {
        public string? LessonName_ka { get; set; }
        public string? LessonName_en { get; set; }
        public ICollection<NewFullLearnModel> newLearnModels { get; set; }
    }
}
