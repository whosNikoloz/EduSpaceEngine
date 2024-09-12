using AutoMapper;
using EduSpaceEngine.Data;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduSpaceEngine.Services.Learn.Lesson
{
    public class LessonService : ILessonService
    {

        private readonly DataDbContext _db;
        private readonly IMapper _mapper;
        public LessonService(DataDbContext db, IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task<IActionResult> GetLessonsBySubjectId(int subjectId)
        {
            if (!_db.Subjects.Any(u => u.SubjectId == subjectId))
            {
                return new NotFoundObjectResult("Subject Not Found");
            }
            var lessons = await _db.Lessons
                .Include(u => u.Subject)
                .Where(u => u.SubjectId == subjectId)
                .ToListAsync();

            if (lessons == null)
            {
                return new NotFoundObjectResult("Lessons not found");
            }
            return new OkObjectResult(lessons);
        }

        public async Task<IActionResult> CreateLessonAsync(LessonDto lessonDto, int subjectid)
        {

            var subject = await _db.Subjects.FirstOrDefaultAsync(u => u.SubjectId == subjectid);

            if (subject == null)
            {
                return new NotFoundObjectResult("Subject Not Found");
            }

            if (_db.Lessons.Any(u => u.LessonName_ka == lessonDto.LessonName_ka && u.SubjectId == subject.SubjectId))
            {
                return new ConflictObjectResult("Lesson Already Exists");
            }

            try
            {
                LessonModel newLesson = _mapper.Map<LessonModel>(lessonDto);
                newLesson.SubjectId = subject.SubjectId;
                _db.Lessons.Add(newLesson);
                _db.SaveChanges();
                return new OkObjectResult(newLesson);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        public async Task<IActionResult> DeleteLessonAsync(int lessonId)
        {
            var lesson = _db.Lessons.FirstOrDefault(u => u.LessonId == lessonId);
            if (lesson == null)
            {
                return new NotFoundObjectResult("Not Found");
            }
            try
            {
                _db.Lessons.Remove(lesson);
                await _db.SaveChangesAsync();
                return new OkObjectResult("Deleted Lesson");
            }
            catch(Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        public async Task<IActionResult> GetAllLessonsAsync()
        {
           
            try
            {
                var lessons = await _db.Lessons
                  .Include(u => u.Subject)
                  .Include(u => u.LearnMaterial)
                  .ToListAsync();
                if (lessons == null)
                {
                    return new NotFoundObjectResult("Lessons not found");
                }
                return new OkObjectResult(lessons);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }

        public async Task<IActionResult> GetLessonByIdAsync(int lessonId)
        {
            var lesson = await _db.Lessons
                .Include(u => u.Subject)
                .Include(u => u.LearnMaterial)
                .FirstOrDefaultAsync(u => u.LessonId == lessonId);

            if (lesson == null)
            {
                return new NotFoundObjectResult("Lesson not found");
            }
            return new OkObjectResult(lesson);
        }

        public async Task<IActionResult> UpdateLessonAsync(int lessonId, LessonDto lessonDto)
        {
            var lesson = await _db.Lessons.FirstOrDefaultAsync(u => u.LessonId == lessonId);

            if (lesson == null)
            {
                return new NotFoundObjectResult("Lesson not found");
            }
            try
            {
                _mapper.Map(lessonDto, lesson);
                _db.Lessons.Update(lesson);
                _db.SaveChanges();
                return new OkObjectResult(lesson);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
        }
    }
}
