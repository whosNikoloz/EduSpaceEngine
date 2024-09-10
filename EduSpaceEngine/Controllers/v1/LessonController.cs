using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;


namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class LessonController : ControllerBase
    {
        private readonly ILevelService _levelService;

        public LessonController(ILevelService levelService)
        {
            _levelService = levelService;   
        }

        [HttpGet("Lessons")]
        public async Task<ActionResult<IEnumerable<LessonModel>>> Lessons()
        {

            var lessons = await _context.Lessons
                .Include(u => u.Subject)
                .Include(u => u.LearnMaterial)
                .ToListAsync();

            return Ok(lessons);
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ გაკვეთილის მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="lessonid">სუბიექტის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Lesson/{lessonid}")]
        public async Task<IActionResult> Lesson(int lessonid)
        {
            var lesson = await _context.Lessons
                .Include(u => u.Subject)
                .Include(u => u.LearnMaterial)
                 .ThenInclude(t => t.Test)
                    .ThenInclude(t => t.Answers)
                .FirstOrDefaultAsync(u => u.LessonId == lessonid);

            return Ok(lesson);
        }

        /// <summary>
        /// ამატებს ახალ საგანს.
        /// </summary>
        /// <param name="newlesson">დამატებული ახალი გაკვეთილის ინფორმაცია.</param>
        /// <param name="subjectname">თემის სახელწოდება, რომელსაც ეკუთვნის საგანი.</param>
        [HttpPost("Lesson"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddLesson(NewLessonModel newlesson, string subjectname_en)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = await _context.Subjects.FirstOrDefaultAsync(u => u.SubjectName_en == subjectname_en);

            if (subject == null)
            {
                return NotFound("Subject Not Found");
            }

            if (_context.Lessons.Any(u => u.LessonName_ka == newlesson.LessonName_ka && u.SubjectId == subject.SubjectId))
            {
                return BadRequest("Lesson Already Exists");
            }

            var lesson = new LessonModel
            {
                LessonName_ka = newlesson.LessonName_ka,
                LessonName_en = newlesson.LessonName_en,
                SubjectId = subject.SubjectId,
            };

            _context.Lessons.Add(lesson);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return Ok(lesson);
        }

        /// <summary>
        /// არედაქტირებს არსებულ საგანს.
        /// </summary>
        /// <param name="newlesson">განახლებული ინფორმაცია გაკვეთილისთვის.</param>
        /// <param name="lessonid">რედაქტირებადი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("Lessons/{lessonid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditLesson(NewLessonModel newlesson, int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesson = await _context.Lessons.FirstOrDefaultAsync(u => u.LessonId == lessonid);

            if (lesson == null)
            {
                return NotFound("Lesson Not Found");
            }

            lesson.LessonName_ka = newlesson.LessonName_ka;
            lesson.LessonName_en = newlesson.LessonName_en;
            lesson.SubjectId = lesson.SubjectId;

            await _context.SaveChangesAsync();

            return Ok(lesson);
        }

        /// <summary>
        /// შლის კონკრეტულ გაკვეთილს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="lessonid">წაშლილი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("Lessons/{lesson}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLesson(int lessonid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var lesson = await _context.Lessons.FindAsync(lessonid);

                if (lesson == null)
                {
                    return NotFound("Lesson Not Found");
                }

                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

                return Ok("Lesson and associated entities removed");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting lesson: {ex.Message}");
            }
        }

    }
}
