using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using Microsoft.AspNetCore.Mvc;

namespace EduSpaceEngine.Services.Learn.Lesson
{
    public class LessonService : ILessonService
    {

        private readonly DataDbContext _db;
        public LessonService(DataDbContext db)
        {
            _db = db;
        }

        public Task<IActionResult> CreateLessonAsync(LessonDto lessonDto)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteLessonAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetAllLessonsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetLessonByIdAsync(int lessonId)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateLessonAsync(int lessonId, LessonDto lessonDto)
        {
            throw new NotImplementedException();
        }
    }
}
