using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;

namespace EduSpaceEngine.Services.Learn.Lesson
{
    public class LessonService : ILessonService
    {

        private readonly DataDbContext _db;
        public LessonService(DataDbContext db)
        {
            _db = db;
        }
    }
}
