using Asp.Versioning;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Services.Learn.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EduSpaceEngine.Dto.Learn;
using EduSpaceEngine.Model.Learn.Request;
using Microsoft.EntityFrameworkCore;
using EduSpaceEngine.Services.Learn.Course;


namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;   
        }

        [HttpGet("Courses")]
        public async Task<IActionResult> Courses()
        {
            var response = await _courseService.GetAllCoursesAsync();

        }



        [HttpGet("Courses/CourseName/{notFormattedCourseName}")]
        public async Task<IActionResult> CourseFormattedName(string notFormattedCourseName, string lang = "ka")
        {
            // Determine which column to use based on the language
            string columnName = $"CourseName_{lang}";

            var course = await _context.Courses
                .Where(u => u.FormattedCourseName == notFormattedCourseName)
                .Select(u => new { CourseName = EF.Property<string>(u, columnName) })
                .FirstOrDefaultAsync();

            if (course == null || string.IsNullOrWhiteSpace(course.CourseName))
            {
                return NotFound("Course Not Found");
            }

            return Ok(course.CourseName);
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ კურსს თავისი უნიკალური იდენტიფიკატორი
        /// </summary>
        /// <param name="courseid">კურსის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Course/{courseName}")]
        public async Task<IActionResult> Course(string courseName, string lang = "ka")
        {
            var course = await _context.Courses
        .Include(u => u.Level)
        .Include(u => u.Subjects).ThenInclude(s => s.Lessons)
        .Include(u => u.Enrollments)
        .FirstOrDefaultAsync(u => u.FormattedCourseName == courseName);

            if (course == null)
            {
                return NotFound("Course Not Found");
            }

            // Determine which property to return based on the language for course name and description
            string courseNameProperty = lang == "en" ? course.CourseName_en : course.CourseName_ka;
            string descriptionProperty = lang == "en" ? course.Description_en : course.Description_ka;

            // Determine which property to return based on the language for subject name and description
            Func<SubjectModel, string?> subjectNameSelector = lang == "en" ? (Func<SubjectModel, string?>)(s => s.SubjectName_en) : s => s.SubjectName_ka;
            Func<SubjectModel, string?> subjectDescriptionSelector = lang == "en" ? (Func<SubjectModel, string?>)(s => s.Description_en) : s => s.Description_ka;

            // Determine which property to return based on the language for lesson name
            Func<LessonModel, string?> lessonNameSelector = lang == "en" ? (Func<LessonModel, string?>)(l => l.LessonName_en) : l => l.LessonName_ka;

            // Create a new object with language-specific properties
            var courseDto = new
            {
                CourseId = course.CourseId,
                CourseName = courseNameProperty,
                Description = descriptionProperty,
                FormattedCourseName = course.FormattedCourseName,
                CourseLogo = course.CourseLogo,
                Level = course.Level,
                Subjects = course.Subjects.Select(s => new
                {
                    SubjectId = s.SubjectId,
                    SubjectName = subjectNameSelector(s),
                    Description = subjectDescriptionSelector(s),
                    LogoURL = s.LogoURL,
                    Lessons = s.Lessons.Select(l => new
                    {
                        LessonId = l.LessonId,
                        LessonName = lessonNameSelector(l),
                        LearnMaterial = l.LearnMaterial
                    })
                }),
                Enrollments = course.Enrollments
            };

            return Ok(courseDto);
        }


        /// <summary>
        /// Adds a new course.
        /// </summary>
        /// <param name="newCourseModel">დამატებული ახალი კურსის ინფორმაცია.</param>
        [HttpPost("Course")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddCourse(NewCourseModel newCourseModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Courses.Any(u => u.CourseName_ka == newCourseModel.CourseName_ka))
            {
                return BadRequest("Course Already Exists");
            }

            var course = new CourseModel
            {
                CourseName_ka = newCourseModel.CourseName_ka,
                CourseName_en = newCourseModel.CourseName_en,
                Description_ka = newCourseModel.Description_ka,
                Description_en = newCourseModel.Description_en,
                CourseLogo = newCourseModel.CourseLogo,
                FormattedCourseName = newCourseModel.FormattedCourseName,
                LevelId = newCourseModel.LevelId
            };

            _context.Courses.Add(course);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return Ok(course);
        }

        /// <summary>
        /// არედაქტირებს არსებულ კურსს.
        /// </summary>
        /// <param name="newcourse">კურსის განახლებული ინფორმაცია.</param>
        /// <param name="courseid">რედაქტირებადი კურსის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("Courses/{courseid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditCourse(NewCourseModel newcourse, int courseid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(u => u.CourseId == courseid);

            if (course == null)
            {
                return NotFound("Course Not Found");
            }

            course.CourseName_ka = newcourse.CourseName_ka;
            course.CourseName_en = newcourse.CourseName_en;
            course.Description_ka = newcourse.Description_ka;
            course.Description_en = newcourse.Description_en;
            course.LevelId = newcourse.LevelId;

            await _context.SaveChangesAsync();

            return Ok(course);
        }

        /// <summary>
        /// შლის კონკრეტულ კურსს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="courseid">კურსის უნიკალური იდენტიფიკატორი, რომელიც უნდა წაიშალოს.</param>
        [HttpDelete("Courses/{courseid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCourse(int courseid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var course = await _context.Courses.Include(c => c.Subjects)
                                                   .ThenInclude(s => s.Lessons)
                                                   .ThenInclude(l => l.LearnMaterial)
                                                   .FirstOrDefaultAsync(c => c.CourseId == courseid);

                if (course == null)
                {
                    return NotFound();
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting course: {ex.Message}");
            }
        }

    }
}
