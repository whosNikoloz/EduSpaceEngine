using Asp.Versioning;
using EduSpaceEngine.Data;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.FullCourseReq;
using EduSpaceEngine.Model.Learn.Request;
using EduSpaceEngine.Model.Learn.Test;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EduSpaceEngine.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class LearnController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public LearnController(DataDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        // ---------- Levels ----------

        /// <summary>
        /// იღებს დონეების სიას.
        /// </summary>
        [HttpGet("Levels")]
        public async Task<ActionResult<IEnumerable<LevelModel>>> Levels()
        {
            var levels = await _context.Levels
                .ToListAsync();

            return Ok(levels);
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ დონეს თავისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="levelid">დონის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Level/{levelid}")]
        public async Task<ActionResult<LevelModel>> Level(int levelid)
        {
            var level = await _context.Levels
                .Include(u => u.Courses)
                .FirstOrDefaultAsync(u => u.LevelId == levelid);

            return Ok(level);
        }

        /// <summary>
        /// ამატებს ახალ დონეს.
        /// </summary>
        /// <param name="newLevel">დამატებული ახალი დონის ინფორმაცია.</param>
        [HttpPost("Levels")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AddLevel(NewLevelModel newLevel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Levels.Any(u => u.LevelName_ka == newLevel.LevelName_ka))
            {
                return NotFound(new
                {
                    successful = false,
                    error = "Level Already Exists"
                });
            }

            var level = new LevelModel
            {
                LevelName_ka = newLevel.LevelName_ka,
                LevelName_en = newLevel.LevelName_en,
                Description_ka = newLevel.Description_ka,
                Description_en = newLevel.Description_en,
                LogoURL = newLevel.LogoURL,
            };

            _context.Levels.Add(level);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                successful = true,
                level = level,
            });

        }

        /// <summary>
        /// არედაქტირებს არსებულ დონეს.
        /// </summary>
        /// <param name="newLevel">განახლებული ინფორმაცია დონისთვის.</param>
        /// <param name="levelid">რედაქტირებადი დონის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("Levels/{levelid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditLevel(NewLevelModel newLevel, int levelid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var level = await _context.Levels.FirstOrDefaultAsync(u => u.LevelId == levelid);

            if (level == null)
            {
                return NotFound(new
                {
                    successful = true,
                    error = "Level Already Exists",
                });
            }

            level.LevelName_en = newLevel.LevelName_en;
            level.LevelName_ka = newLevel.LevelName_ka;
            level.LogoURL = newLevel.LogoURL;
            level.Description_ka = newLevel.Description_ka;
            level.Description_en = newLevel.Description_en;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                successful = true,
                level = level,
            });
        }

        /// <summary>
        /// შლის კონკრეტულ დონეს თავისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="levelid">წაშლილი დონის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("Levels/{levelid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLevel(int levelid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var level = await _context.Levels
                    .SingleOrDefaultAsync(l => l.LevelId == levelid);

                if (level == null)
                {
                    return NotFound("Level Not Found");
                }

                _context.Levels.Remove(level);

                await _context.SaveChangesAsync();

                return Ok("Level and all associated entities removed");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting level: {ex.Message}");
            }
        }

        // ---------- Courses ----------

        /// <summary>
        /// იღებს კურსების სიას.
        /// </summary>
        [HttpGet("Courses")]
        public async Task<IActionResult> Courses(string lang = "ka")
        {
            var courses = await _context.Courses.ToListAsync();

            var coursesResponse = new List<object>();

            foreach (var course in courses)
            {
                string courseName;
                string description;

                if (lang.Equals("ka", StringComparison.OrdinalIgnoreCase))
                {
                    courseName = course.CourseName_ka ?? course.CourseName_en;
                    description = course.Description_ka ?? course.Description_en;
                }
                else // Default to English if language is not specified or unsupported
                {
                    courseName = course.CourseName_en ?? course.CourseName_ka;
                    description = course.Description_en ?? course.Description_ka;
                }

                var courseResponse = new
                {
                    CourseId = course.CourseId,
                    CourseName = courseName,
                    Description = description,
                    CourseLogo = course.CourseLogo,
                    FormattedCourseName = course.FormattedCourseName,
                    LevelId = course.LevelId
                };

                coursesResponse.Add(courseResponse);
            }

            return Ok(coursesResponse);
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



        // ---------- Subjects ----------

        /// <summary>
        /// ამოიღებს საგნების სიას.
        /// </summary>


        [HttpGet("Subjects")]
        public async Task<ActionResult<IEnumerable<SubjectModel>>> Subjects()
        {

            var subjects = await _context.Subjects
                .Include(u => u.Course)
                .ToListAsync();

            return Ok(subjects);
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ საგანს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="subjectid">სუბიექტის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Subject/{subjectid}")]
        public async Task<IActionResult> Subject(int subjectid)
        {
            var subject = await _context.Subjects
                .Include(u => u.Course)
                .Include(u => u.Lessons)
                .FirstOrDefaultAsync(u => u.SubjectId == subjectid);

            return Ok(subject);
        }

        /// <summary>
        /// ამატებს ახალ საგანს.
        /// </summary>
        /// <param name="newsubject">დამატებული ახალი თემის ინფორმაცია.</param>
        /// <param name="coursename">კურსის სახელწოდება, რომელსაც ეკუთვნის საგანი.</param>
        [HttpPost("Subject"), Authorize(Roles = "admin")]
        public async Task<IActionResult> AddSubject(NewSubjectModel newsubject, string coursename)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var course = await _context.Courses.FirstOrDefaultAsync(u => u.FormattedCourseName == coursename);

            if (course == null)
            {
                return NotFound("Course Not Found");
            }

            if (_context.Subjects.Any(u => u.SubjectName_ka == newsubject.SubjectName_ka && u.CourseId == course.CourseId))
            {
                return BadRequest("Subject Already Exists");
            }

            var subject = new SubjectModel
            {
                SubjectName_ka = newsubject.SubjectName_ka,
                SubjectName_en = newsubject.SubjectName_en,
                Description_ka = newsubject.Description_ka,
                Description_en = newsubject.Description_en,
                LogoURL = newsubject.LogoURL,
                CourseId = course.CourseId,
            };

            _context.Subjects.Add(subject);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return Ok(subject);
        }

        /// <summary>
        /// არედაქტირებს არსებულ საგანს.
        /// </summary>
        /// <param name="newsubject">განახლებული ინფორმაცია თემისთვის.</param>
        /// <param name="subjectid">რედაქტირებადი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpPut("Subjects/{subjectid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> EditSubject(NewSubjectModel newsubject, int subjectid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var subject = await _context.Subjects.FirstOrDefaultAsync(u => u.SubjectId == subjectid);

            if (subject == null)
            {
                return NotFound("Subject Not Found");
            }

            subject.SubjectName_ka = newsubject.SubjectName_ka;
            subject.SubjectName_en = newsubject.SubjectName_en;
            subject.Description_ka = newsubject.Description_ka;
            subject.Description_en = newsubject.Description_en;
            subject.LogoURL = newsubject.LogoURL;

            await _context.SaveChangesAsync();

            return Ok(subject);
        }

        /// <summary>
        /// შლის კონკრეტულ საგანს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="subjectid">წაშლილი საგნის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("Subjects/{subjectid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteSubject(int subjectid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var subject = await _context.Subjects.FindAsync(subjectid);

                if (subject == null)
                {
                    return NotFound("Subject Not Found");
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                return Ok("Subject and associated entities removed");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting subject: {ex.Message}");
            }
        }


        // ---------- Lessons ----------

        /// <summary>
        /// ამოიღებს სწავლების სიას.
        /// </summary>


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

        // ---------- Tests ----------

        /// <summary>
        /// ამოიღებს ტესტების სიას.
        /// </summary>
        [HttpGet("Tests")]
        public async Task<ActionResult<IEnumerable<TestModel>>> GetTests()
        {
            return await _context.Tests.Include(t => t.Answers).ToListAsync();
        }

        /// <summary>
        /// ამოიღებს კონკრეტულ ტესტს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">ტესტის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("Tests/{id}")]
        public async Task<ActionResult<TestModel>> GetTest(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var test = await _context.Tests.Include(t => t.Answers).FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            return test;
        }

        /// <summary>
        /// ამატებს ახალ ტესტს.
        /// </summary>
        /// <param name="test">დამატებული ახალი ტესტის ინფორმაცია.</param>
        [HttpPost("Tests/{LearnId}"), Authorize(Roles = "admin")]
        public async Task<ActionResult<TestModel>> PostTest(NewTestModel test, int LearnId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var learn = await _context.Learn.FirstOrDefaultAsync(u => u.LearnId == LearnId);

            if (learn == null)
            {
                return NotFound("Learn not found");
            }

            var testModel = new TestModel
            {
                Instruction = test.Instruction,
                Code = test.Code,
                Question = test.Question,
                Hint = test.Hint,
                LearnId = learn.LearnId,
            };

            _context.Tests.Add(testModel);
            await _context.SaveChangesAsync();

            // Set the Learn's TestId to the newly created test's ID
            learn.TestId = testModel.TestId;

            // Update the Learn entity with the TestId change
            _context.Learn.Update(learn);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTest", new { id = testModel.TestId }, testModel);
        }

        /// <summary>
        /// არედაქტირებს არსებულ ტესტს.
        /// </summary>
        /// <param name="id">რედაქტირებადი ტესტის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="test">ტესტის განახლებული ინფორმაცია.</param>
        [HttpPut("Tests/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutTest(int id, TestModel test)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != test.TestId)
            {
                return BadRequest();
            }

            _context.Entry(test).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// შლის კონკრეტულ ტესტს მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">წაშლილი ტესტის უნიკალური იდენტიფიკატორი.</param>
        [HttpDelete("Tests/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTest(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var test = await _context.Tests.Include(t => t.Answers)
                                               .FirstOrDefaultAsync(t => t.TestId == id);

                if (test == null)
                {
                    return NotFound();
                }

                _context.Tests.Remove(test);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting test: {ex.Message}");
            }
        }

        // ---------- Answers ----------

        /// <summary>
        /// Retrieves answers for a specific test based on its unique identifier.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        [HttpGet("answers/{testid}")]
        public async Task<ActionResult<TestModel>> GetAnswers(int testId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var test = await _context.Tests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
        }

        /// <summary>
        /// Adds an answer to a specific test.
        /// </summary>
        /// <param name="testId">The unique identifier of the test.</param>
        /// <param name="answer">The information of the added answer.</param>
        [HttpPost("{testId}/answers"), Authorize(Roles = "admin")]
        public async Task<ActionResult<TestModel>> AddAnswerToTest(int testId, NewTestAnswerModel answer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var test = await _context.Tests
                .Include(t => t.Answers)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null)
            {
                return NotFound();
            }

            if (test.Answers == null)
            {
                test.Answers = new List<TestAnswerModel>();
            }

            var Answer = new TestAnswerModel
            {
                Option = answer.Option,
                IsCorrect = answer.IsCorrect,
                TestId = testId,
            };

            _context.TestAnswers.Add(Answer);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                // You might inform the user about the concurrency issue and prompt for action
            }

            return CreatedAtAction(nameof(GetTest), new { id = test.TestId }, test);
        }

        /// <summary>
        /// Deletes a specific answer based on its unique identifier.
        /// </summary>
        /// <param name="answerid">The unique identifier of the deleted answer.</param>
        [HttpDelete("answers/{answerid}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteAnswers(int answerid)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var answer = await _context.TestAnswers.FindAsync(answerid);

                if (answer == null)
                {
                    return NotFound();
                }

                _context.TestAnswers.Remove(answer);
                await _context.SaveChangesAsync();

                return Ok("Deleted");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                // You might inform the user about the concurrency issue and prompt for action
                return BadRequest("Concurrency error occurred while deleting the answer.");
            }
            catch (Exception ex)
            {
                // Log any other unexpected exceptions
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting answer: {ex.Message}");
            }
        }

        // ---------- Learn Materials ----------

        /// <summary>
        /// ამოიღებს სასწავლო მასალის ჩამონათვალს.
        /// </summary>
        [HttpGet("LearnMaterials")]
        public async Task<ActionResult<IEnumerable<LearnModel>>> GetLearns()
        {
            return await _context.Learn.Include(t => t.Test).ThenInclude(t => t.Answers).ToListAsync();
        }


        /// <summary>
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("LearnMaterialByLesson/{LessonId}")]
        public async Task<ActionResult> GetLearnmaterial(int LessonId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lessons = await _context.Learn
                .Include(t => t.Test)
                    .ThenInclude(t => t.Answers) // Include Answers
                .Where(t => t.LessonId == LessonId)
                .ToListAsync();

            if (lessons == null || lessons.Count == 0)
            {
                return NotFound();
            }

            return Ok(lessons);
        }



        /// <summary>
        /// ამოიღებს კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        [HttpGet("LearnMaterial/{id}")]
        public async Task<ActionResult<LearnModel>> GetLearn(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var learn = await _context.Learn
                .Include(t => t.Test)
                    .ThenInclude(t => t.Answers) // Include Answers
                .FirstOrDefaultAsync(t => t.LearnId == id);

            if (learn == null)
            {
                return NotFound();
            }

            return learn;
        }

        /// <summary>
        /// ამატებს ახალ სასწავლო მასალას.
        /// </summary>
        /// <param name="learn">დამატებული ახალი სასწავლო მასალის ინფორმაცია.</param>
        /// <param name="subjectname">საგნის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        /// <param name="coursename">კურსის სახელწოდება, რომელსაც მიეკუთვნება სასწავლო მასალა.</param>
        [HttpPost("LearnMaterial"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PostLearn(NewLearnModel learn, int LessonId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var lesson = await _context.Lessons.FirstOrDefaultAsync(u => u.LessonId == LessonId);

            if (lesson == null)
            {
                return NotFound();
            }

            if (_context.Learn.Any(u => u.LearnName == learn.LearnName))
            {
                return BadRequest("LearnMaterial Already Exists");
            }

            var Learn = new LearnModel
            {
                LearnName = learn.LearnName,
                Content = learn.Content,
                Code = learn.Code,
                LessonId = lesson.LessonId,
            };

            _context.Learn.Add(Learn);
            await _context.SaveChangesAsync();

            return Ok(Learn);
        }

        /// <summary>
        /// ასწორებს არსებულ სასწავლო მასალას.
        /// </summary>
        /// <param name="id">რედაქტირებადი სასწავლო მასალის უნიკალური იდენტიფიკატორი.</param>
        /// <param name="learn">სასწავლო მასალის განახლებული ინფორმაცია.</param>
        [HttpPut("LearnMaterial/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> PutLearn(int id, LearnModel learn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != learn.LearnId)
            {
                return BadRequest();
            }

            _context.Entry(learn).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// შლის კონკრეტულ სასწავლო მასალას მისი უნიკალური იდენტიფიკატორის მიხედვით.
        /// </summary>
        /// <param name="id">სასწავლო მასალის უნიკალური იდენტიფიკატორი, რომელიც უნდა წაიშალოს.</param>
        [HttpDelete("LearnMaterial/{id}"), Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteLearn(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var learn = await _context.Learn.FindAsync(id);

                if (learn == null)
                {
                    return NotFound();
                }

                _context.Learn.Remove(learn);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a way that makes sense for your application
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting learn material: {ex.Message}");
            }
        }



        [HttpPost("FullCourse")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCourse([FromBody] FullCourseModel newCourse)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (_context.Courses.Any(c => c.FormattedCourseName == newCourse.FormattedCourseName))
            {
                return BadRequest("Course Already Exists");
            }

            var course = new CourseModel
            {
                CourseName_ka = newCourse.CourseName_ka,
                CourseName_en = newCourse.CourseName_en,
                FormattedCourseName = newCourse.FormattedCourseName,
                Description_ka = newCourse.Description_ka,
                Description_en = newCourse.Description_en,
                CourseLogo = newCourse.CourseLogo,
                LevelId = newCourse.LevelId,
                Subjects = new List<SubjectModel>()
            };

            foreach (var newSubject in newCourse.newSubjectModels)
            {
                var subject = new SubjectModel
                {
                    SubjectName_ka = newSubject.SubjectName_ka,
                    SubjectName_en = newSubject.SubjectName_en,
                    Description_ka = newSubject.Description_ka,
                    Description_en = newSubject.Description_en,
                    LogoURL = newSubject.LogoURL,
                    Lessons = new List<LessonModel>()
                };

                foreach (var newLesson in newSubject.newLessonModels)
                {
                    var lesson = new LessonModel
                    {
                        LessonName_ka = newLesson.LessonName_ka,
                        LessonName_en = newLesson.LessonName_en,
                        LearnMaterial = new List<LearnModel>()
                    };

                    foreach (var newLearn in newLesson.newLearnModels)
                    {
                        var learn = new LearnModel
                        {
                            LearnName = newLearn.LearnName,
                            Content = newLearn.Content,
                            Code = newLearn.Code,
                            LessonId = lesson.LessonId // Set the foreign key relationship
                        };

                        if (newLearn.newTestModels != null && newLearn.newTestModels.Any())
                        {
                            var test = new TestModel
                            {
                                Instruction = newLearn.newTestModels.First().Instruction,
                                Question = newLearn.newTestModels.First().Question,
                                Code = newLearn.newTestModels.First().Code,
                                Hint = newLearn.newTestModels.First().Hint,
                                Answers = new List<TestAnswerModel>(),
                                LearnId = learn.LearnId // Set the foreign key relationship
                            };

                            foreach (var newAnswer in newLearn.newTestModels.First().Answers)
                            {
                                var answer = new TestAnswerModel
                                {
                                    Option = newAnswer.Option,
                                    IsCorrect = newAnswer.IsCorrect,
                                    TestId = test.TestId // Set the foreign key relationship
                                };

                                test.Answers.Add(answer);
                            }

                            learn.Test = test;
                        }

                        lesson.LearnMaterial.Add(learn);
                    }

                    subject.Lessons.Add(lesson);
                }

                course.Subjects.Add(subject);
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(course);
        }
    }
}
