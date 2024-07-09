namespace EduSpaceEngine.Model.Learn.FullCourseReq
{
    public class NewFullSubjectModel
    {
        public string? SubjectName_ka { get; set; }
        public string? SubjectName_en { get; set; }

        public string? Description_ka { get; set; }
        public string? Description_en { get; set; }

        public string? LogoURL { get; set; }

        public ICollection<NewFullLessonModel> newLessonModels { get; set; }

    }
}
