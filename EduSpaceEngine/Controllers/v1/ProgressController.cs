using Asp.Versioning;
using EduSpaceEngine.Data;
using EduSpaceEngine.Model.Learn;
using EduSpaceEngine.Model.Learn.Request;
using EduSpaceEngine.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Numerics;
using System.Security.Claims;

namespace EduSpaceEngine.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProgressController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly IConfiguration _configuration;

        public ProgressController(DataDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("GetProgress"), Authorize]
        public async Task<IActionResult> GetProgress([FromQuery] ProgressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; // JWT id check
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; // JWT Role

            if (user == null)
            {
                return BadRequest("User not found.");
            }

            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Unauthorized");
                }
            }

            var progress = await _context.Progress.FirstOrDefaultAsync(u => u.UserId == user.UserId && u.CourseId == request.CourseId);

            if (progress == null)
            {
                var subject = await _context.Subjects.Include(u => u.Lessons).FirstOrDefaultAsync(u => u.CourseId == request.CourseId);

                if (subject == null)
                {
                    return BadRequest("No subjects found for the specified course.");
                }


                var firstLesson = subject.Lessons.OrderBy(l => l.LessonId).FirstOrDefault();


                if (firstLesson == null)
                {
                    return BadRequest("No lessons found for the specified subject.");
                }


                var newProgress = new ProgressModel
                {
                    UserId = user.UserId,
                    CourseId = (int)request.CourseId,
                    SubjectId = subject.SubjectId, // Assuming 1 is the ID of the first subject
                    LessonId = firstLesson.LessonId,  // Assuming 1 is the ID of the first lesson
                };

                _context.Progress.Add(newProgress);
                await _context.SaveChangesAsync();

                var response = new
                {
                    progressId = newProgress.ProgressId,
                    subjectId = newProgress.SubjectId,
                    lessonId = newProgress.LessonId,
                    complete = newProgress.Complete,
                };

                return Ok(response);
            }

            var existingProgress = new
            {
                progressId = progress.ProgressId,
                subjectId = progress.SubjectId,
                lessonId = progress.LessonId,
                complete = progress.Complete,
            };

            return Ok(existingProgress);
        }



        [HttpPost("complete-lesson"), Authorize]
        public async Task<IActionResult> CompleteLesson(ProgressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            var progress = await _context.Progress.FirstOrDefaultAsync(u => u.UserId == request.UserId && u.LessonId == request.LessonId );

            if (progress == null)
            {
                var lesson = await _context.Lessons.Include(u => u.Subject).FirstOrDefaultAsync(u => u.LessonId == request.LessonId);

                var NewnextLesson = await _context.Lessons
                    .Where(l => l.SubjectId == lesson.SubjectId && l.LessonId > request.LessonId)
                    .OrderBy(l => l.LessonId)
                    .FirstOrDefaultAsync();

                var newProgress = new ProgressModel
                {
                    UserId = request.UserId,
                    CourseId = lesson.Subject.CourseId,
                    SubjectId = lesson.SubjectId,
                    LessonId = (int)(NewnextLesson?.LessonId ?? request.LessonId), // Use next lesson's ID or the requested lesson's ID if there is no next lesson.
                };

                _context.Progress.Add(newProgress);

                await _context.SaveChangesAsync();

                return Ok("User started a new course");
            }

            // Get the next lesson
            var nextLesson = await _context.Lessons.Where(l => l.SubjectId == progress.SubjectId && l.LessonId > progress.LessonId).OrderBy(l => l.LessonId).FirstOrDefaultAsync();

            if (nextLesson != null)
            {
                // Update the LessonId in progress to the next lesson's Id
                progress.LessonId = nextLesson.LessonId;
                _context.Entry(progress).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(progress);
            }
            else
            {
                // If there are no more lessons, complete the subject
                var subjectRequest = new ProgressRequest { UserId = request.UserId, SubjectId = progress.SubjectId };
                return await CompleteSubject(subjectRequest);
            }
        }

        [HttpPost("complete-subject"), Authorize]
        public async Task<IActionResult> CompleteSubject(ProgressRequest request)
        {
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user from the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (user == null)
            {
                return BadRequest("User not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorization invalid");
                }
            }

            // Update the user's progress for the specified subject
            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.SubjectId == request.SubjectId);
            if (subject == null)
            {
                return BadRequest("Subject not found.");
            }

            var progress = await _context.Progress.FirstOrDefaultAsync(u => u.UserId == request.UserId && u.SubjectId == request.SubjectId);
            if (progress == null)
            {
                return NotFound("Progres not found");
            }

            progress.Lesson = null;

            var nextSubject = await _context.Subjects.Include(l => l.Lessons)
                .Where(l => l.CourseId == progress.CourseId && l.SubjectId > progress.SubjectId)
                .OrderBy(l => l.SubjectId)
                .FirstOrDefaultAsync();

            if (nextSubject != null)
            {
                // Update the LessonId in progress to the next lesson's Id
                progress.SubjectId = nextSubject.SubjectId;

                // Update LessonId with the first LessonId from nextSubject
                progress.LessonId = (int)(nextSubject.Lessons.FirstOrDefault()?.LessonId);

                _context.Entry(progress).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(progress);
            }
            else
            {
                // If there are no more lessons, complete the subject
                var courseRequest = new ProgressRequest { UserId = request.UserId, CourseId = progress.CourseId };
                return await CompleteCourse(courseRequest);
            }
        }

        /// <summary>
        /// აღნიშნავს კურსს მომხმარებლისთვის დასრულებულად.
        /// </summary>
        /// <param name="request">დასრულების მოთხოვნა, რომელიც შეიცავს მომხმარებლის და კურსის ინფორმაციას.</param>
        [HttpPost("complete-course"), Authorize]
        public async Task<IActionResult> CompleteCourse(ProgressRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserId);
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value; //JWT id ჩეკავს
            var JWTRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value; //JWT Role

            if (user == null)
            {
                return BadRequest("user not found.");
            }
            if (userId != user.UserId.ToString())
            {
                if (JWTRole != "admin")
                {
                    return BadRequest("Authorize invalid");
                }
            }

            var course = await _context.Subjects.FirstOrDefaultAsync(s => s.CourseId == request.CourseId);
            if (course == null)
            {
                return BadRequest("course not found.");
            }

            var progress = await _context.Progress.FirstOrDefaultAsync(u => u.UserId == request.UserId && u.CourseId == request.CourseId);
            if (progress == null)
            {
                return NotFound("Progres not found");
            }

            progress.Complete = true;

            await _context.SaveChangesAsync();

            return Ok("Course complited");
        }

        

    }
}
