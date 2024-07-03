namespace EduSpaceEngine.Model.Learn.Request
{
    public class ProgressRequest
    {
        public int UserId { get; set; }
        public int? SubjectId { get; set; }
        public int? CourseId { get; set; }
        public int? LessonId { get; set; }
    }
}
